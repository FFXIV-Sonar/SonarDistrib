using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Threading.Tasks;
using Sonar.Models;
using System.Net.Sockets;
using System.Threading;
using Sonar.Utilities;
using MessagePack;
using System.Net.WebSockets;
using Sonar.Messages;
using Sonar.Enums;
using System.Threading.Tasks.Dataflow;
using Sonar.Config;
using Sonar.Sockets;
using Sonar.Trackers;
using static Sonar.Utilities.UnixTimeHelper;
using Sonar.Services;
using Sonar.Logging;
using System.Net;
using System.Net.Http;
using static Sonar.SonarConstants;
using Sonar.Data;
using Sonar.Data.Details;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Collections.Immutable;
using Container = DryIoc.Container;
using DryIoc;
using Sonar.Connections;
using System.Reflection;
using System.Linq;
using DryIoc.MefAttributedModel;

namespace Sonar
{
    /// <summary>Main entry point of Sonar</summary>
    public sealed partial class SonarClient : IDisposable, IAsyncDisposable
    {
        private readonly Container _container = new();

        private readonly object _metaLock = new();
        private readonly TickerService ticker;
        private readonly PingService pinger;
        internal ClientModifiers Modifiers = ClientModifiers.Defaults;

        private ClientIdentifier ClientIdentifier { get; set; }
        private HardwareIdentifier HardwareIdentifier { get; set; } = IdentifierUtils.GetHardwareIdentifier();

        private SonarVersion versionInfo = new();
        /// <summary>
        /// Version information
        /// </summary>
        public SonarVersion VersionInfo
        {
            get => Unsafe.As<SonarVersion>(this.versionInfo.Clone());
            set
            {
                lock (this._metaLock)
                {
                    this.versionInfo = Unsafe.As<SonarVersion>(value.Clone());
                    this.versionInfo.ResetSonarVersion();
                    this.Connection.SendIfConnected(value);
                }
            }
        }

        private PlayerInfo? _playerInfo;
        /// <summary>Current <see cref="PlayerInfo"/></summary>
        public PlayerInfo? PlayerInfo => this._playerInfo;

        /// <summary>Updates <see cref="PlayerInfo"/></summary>
        /// <returns>Update successful</returns>
        public bool UpdatePlayerInfo(PlayerInfo info)
        {
            if (info.Equals(this._playerInfo)) return false;
            lock (this._metaLock)
            {
                this._playerInfo = info;
                this.Connection.SendIfConnected(info);
            }
            return true;
        }

        private PlayerPlace _playerPlace = new();
        /// <summary> Player location</summary>
        public PlayerPlace PlayerPlace
        {
            get => this._playerPlace;
            set
            {
                lock (this._metaLock)
                {
                    this._playerPlace = value;
                    this.Connection.SendIfConnected(value);
                }
            }
        }

        private SonarConfig configuration;
        /// <summary>
        /// Sonar Configuration. Remember to set this on every modification even if the instance modified is the same.
        /// </summary>
        public SonarConfig Configuration
        {
            get => this.configuration;
            set
            {
                lock (this._metaLock)
                {
                    value.VersionUpdate();
                    this.configuration = value;
                    this.baseLogger.Level = value.LogLevel;
                    this.Connection.SendIfConnected(value);
                }
            }
        }

        /// <summary>Sonar relay trackers</summary>
        public RelayTrackers Trackers { get; }

        /// <summary>Sonar connection information</summary>
        public SonarConnectionManager Connection { get; }

        /// <summary>Current ping in milliseconds</summary>
        public double Ping => this.pinger.Ping;

        /// <summary>Sonar Start Info</summary>
        public SonarStartInfo StartInfo { get; }

        /// <summary>Constructs a <see cref="SonarClient"/>. Only one can exist at a time. Remember to call <see cref="Start"/>.</summary>
        public SonarClient(SonarStartInfo? startInfo = null)
        {
            ClientCreated();

            this._container = new(Rules.Default.With(FactoryMethod.Constructor(false, true)));
            this.PrepareDryIoc();

            this.StartInfo = startInfo ?? new();
            this.StartInfo.Initialize();

            this.Connection = this._container.Resolve<SonarConnectionManager>();
            this.Connection.MessageReceived += this.ReceiveHandler;
            this.Connection.TextReceived += this.TextHandler;
            this.Connection.ConnectedInternal += this.ReadyHandler;

            this.ClientIdentifier = IdentifierUtils.GetClientIdentifier(this.StartInfo);

            this.baseLogger.LogMessage += this.LogHandler;
            this.Logger = new SonarLoggerContext(this.baseLogger, "Sonar");
            this.serverLogger = new SonarLoggerContext(this.baseLogger, "Server");

            this.ticker = new(this);
            this.pinger = new(this);
            this.configuration = new();
            this.Trackers = this._container.Resolve<RelayTrackers>();

            this.ticker.Tick += this.Ticker_Tick;
            this.pinger.Pong += this.Pinger_Pong;

            this.baseLogger.Level = this.configuration.LogLevel;
        }

        private void PrepareDryIoc()
        {
            var assembly = Assembly.GetExecutingAssembly();
            this._container.RegisterInstance(this, setup: Setup.With(preventDisposal: true)); // SonarClient is disposed by the user
            this._container.RegisterExports(assembly);
        }

        /// <summary>Starts the <see cref="SonarClient"/>. Calling this method multiple times has no effect and returns <see cref="false"/></summary>
        public bool Start()
        {
            return this.Connection.Start();
        }

        /// <summary>Request all relay data from the Sonar Server</summary>
        public void RequestRelayData()
        {
            this.Connection.SendIfConnected(() => new RelayDataRequest());
        }

        private void ReadyHandler(SonarConnectionManager arg1, ISonarSocket arg2, uint arg3)
        {
            this.pinger.Poke();
            lock (this._metaLock)
            {
                var messages = new MessageList()
                {
                    new ClientHello()
                    {
                        HardwareIdentifier = this.HardwareIdentifier.Identifier,
                        ClientIdentifier = this.ClientIdentifier.Identifier,
                        Version = this.VersionInfo,
                        SonarSecret = ClientSecret.ReadEmbeddedSecret(typeof(SonarClient).Assembly, "Sonar.Resources.Secret.data"),
                        PluginSecret = this.StartInfo.PluginSecret,
                    },

                    this.Configuration,
                    this.PlayerInfo,
                    this.PlayerPlace,
                    Database.GetDbInfo(),
                };
                this.Connection.SendIfConnected(messages);
            }
        }

        private void ReceiveHandler(SonarConnectionManager arg1, ISonarMessage message)
        {
            switch (message)
            {
                case ClientModifiers modifiers:
                    //if (this.LogVerboseEnabled) this.LogVerbose($"Client Modifiers received: {modifiers}");
                    this.Modifiers.MutateWith(modifiers);
                    break;

                case SonarHeartbeat heartbeat:
                    UnixTimeOffset = (heartbeat.UnixTime - UnixNow) - this.Ping / 2;
                    if (this.LogVerboseEnabled)
                    {
                        var syncMessage = UnixTimeOffset == 0 ? "perfectly synchronized with the server" :
                            $"{Math.Abs(UnixTimeOffset)}ms {(UnixTimeOffset > 0 ? "behind" : "ahead")}";

                        var placeCounts = heartbeat.ClientPlaceCounts;
                        var placeWorldIndex = this.PlayerPlace?.GetIndexKey(Indexes.IndexType.World) ?? string.Empty;
                        var placeDatacenterIndex = this.PlayerPlace?.GetIndexKey(Indexes.IndexType.Datacenter) ?? string.Empty;
                        var placeRegionIndex = this.PlayerPlace?.GetIndexKey(Indexes.IndexType.Region) ?? string.Empty;
                        var placeAudienceIndex = this.PlayerPlace?.GetIndexKey(Indexes.IndexType.Audience) ?? string.Empty;
                        var placeCountsText = string.Join(" / ",
                            $"{placeCounts.GetValueOrDefault(placeWorldIndex)}",
                            $"{placeCounts.GetValueOrDefault(placeDatacenterIndex)}",
                            $"{placeCounts.GetValueOrDefault(placeRegionIndex)}",
                            $"{placeCounts.GetValueOrDefault(placeAudienceIndex)}",
                            $"{placeCounts.GetValueOrDefault("all")}"
                        );

                        var homeCounts = heartbeat.ClientHomeCounts; // TODO: Implement indexes in PlayerInfo
                        var homeWorldIndex = string.Empty;// this.PlayerInfo?.GetIndexKey(Indexes.IndexType.World) ?? string.Empty;
                        var homeDatacenterIndex = string.Empty;// this.PlayerInfo?.GetIndexKey(Indexes.IndexType.Datacenter) ?? string.Empty;
                        var homeRegionIndex = string.Empty;// this.PlayerInfo?.GetIndexKey(Indexes.IndexType.Region) ?? string.Empty;
                        var homeAudienceIndex = string.Empty;// this.PlayerInfo?.GetIndexKey(Indexes.IndexType.Audience) ?? string.Empty;
                        var homeCountsText = string.Join(" / ",
                            $"{homeCounts.GetValueOrDefault(homeWorldIndex)}",
                            $"{homeCounts.GetValueOrDefault(homeDatacenterIndex)}",
                            $"{homeCounts.GetValueOrDefault(homeRegionIndex)}",
                            $"{homeCounts.GetValueOrDefault(homeAudienceIndex)}",
                            $"{homeCounts.GetValueOrDefault("all")}"
                        );

                        this.LogVerbose($"Sonar Heartbeat Received (Player Counts: [Place: {placeCountsText} | Home: {homeCountsText}] | Time is {syncMessage})");
                    }
                    break;

                case SonarPing ping:
                    //if (this.LogDebugEnabled) this.LogDebug($"Ping request received from Server {ping.Sequence}");
                    this.Connection.SendIfConnected(new SonarPong() { Sequence = ping.Sequence });
                    break;

                case SonarLogMessage log:
                    this.serverLogger.Log(log.Message, log.Level);
                    break;

                case SonarMessage msg:
                    if (this.LogErrorEnabled) this.LogError($"Sonar Server Message received: {msg.Message}");
                    try
                    {
                        this.ServerMessage?.Invoke(this, msg.Message);
                    }
                    catch (Exception ex)
                    {
                        if (this.LogErrorEnabled) this.LogError(ex, string.Empty);
                    }
                    break;

                case ClientIdentifier identifier:
                    IdentifierUtils.SaveClientIdentifier(identifier, this.StartInfo);
                    this.ClientIdentifier = identifier;
                    break;

                case SupportResponse response:
                    this.SupportCallback(response);
                    break;

                case SonarDb db:
                    this.LogInformation("Received Sonar Database update from server");
                    this.LogDebug(db.ToString());
                    Database.Instance = db;
                    break;
            }
        }

        private void TextHandler(SonarConnectionManager arg1, string text)
        {
            // NOTE: This should never happen
            if (this.LogInformationEnabled) this.LogInformation($"[Server text] {text}");
        }

        private void Ticker_Tick(TickerService ticker)
        {
            try
            {
                this.Tick?.Invoke(this);
            }
            catch (Exception ex)
            {
                if (this.LogErrorEnabled) this.LogError(ex, string.Empty);
            }
        }
        private void Pinger_Pong(PingService pinger, double ping)
        {
            try
            {
                this.Pong?.Invoke(this, ping);
            }
            catch (Exception ex)
            {
                if (this.LogErrorEnabled) this.LogError(ex, string.Empty);
            }
        }

        public event SonarClientEventHandler? Tick;
        public event SonarClientPingHandler? Pong;

        public event SonarServerMessageHandler? ServerMessage;

#region Disposable Interface
        private int disposed; // Interlocked
        public bool IsDisposed => this.disposed == 1;
        public async ValueTask DisposeAsync()
        {
            if (Interlocked.Exchange(ref this.disposed, 1) == 1) return;

            this.ticker.Tick -= this.Ticker_Tick;
            this.pinger.Pong -= this.Pinger_Pong;

            this.Trackers.Hunts.Dispose();
            this.Trackers.Fates.Dispose();

            await this.ticker.DisposeAsync();
            await this.pinger.DisposeAsync();

            this.baseLogger.LogMessage -= this.LogHandler;
            this._container.Dispose();
        }
        public void Dispose() => this.DisposeAsync().AsTask().Wait();
        ~SonarClient()
        {
            ClientFinalized();
            try
            {
                this.Dispose();
            }
            catch
            {
                /* Swallowed or else .NET will crash */
            }
        }
#endregion
    }
}
