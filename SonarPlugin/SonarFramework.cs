using Dalamud.Game.ClientState.Conditions;
using Dalamud.Plugin.Services;
using DryIocAttributes;
using Microsoft.Extensions.Hosting;
using Sonar;
using System;
using System.Collections.Immutable;
using System.Threading;
using System.Threading.Tasks;

namespace SonarPlugin
{
    [ExportMany]
    [SingletonReuse]
    public sealed class SonarFramework : IHostedService
    {
        private ImmutableArray<Action<SonarFramework>> _updateHandlers = [];
        private ImmutableArray<Action<SonarFramework>> _tickHandlers = [];
        private bool _tick;

        public IFramework Framework { get; }
        private ICondition Condition { get; }
        private SonarClient Client { get; }
        private IPluginLog Logger { get; }

        public SonarFramework(IFramework framework, ICondition condition, SonarClient client, IPluginLog logger)
        {
            this.Framework = framework;
            this.Condition = condition;
            this.Client = client;
            this.Logger = logger;

            this.Logger.Info("Sonar Framework Initialized");
        }

        public uint SafetyLevel { get; private set; }

        public bool IsDuty { get; private set; }

        public bool IsSafe(uint safetyLevel = 2) => this.SafetyLevel >= safetyLevel;

        private void Framework_Update(IFramework framework)
        {
            var safe = !this.Condition[ConditionFlag.BetweenAreas51];
            if (safe) this.SafetyLevel++;
            else this.SafetyLevel = 0;

            this.IsDuty = this.Condition[ConditionFlag.BoundByDuty56];

            this.Framework_UpdateCore(this._updateHandlers.AsSpan(), this);
            if (!Interlocked.CompareExchange(ref this._tick, false, true)) return;
            this.Framework_UpdateCore(this._tickHandlers.AsSpan(), this);
        }

        /// <summary>Sets <see cref="SafetyLevel"/> to <c>0</c>.</summary>
        public void Unsafe() => this.SafetyLevel = 0;

        private void Framework_UpdateCore(ReadOnlySpan<Action<SonarFramework>> handlers, SonarFramework framework)
        {
            foreach (var handler in handlers)
            {
                try
                {
                    handler(framework);
                }
                catch (Exception ex)
                {
                    this.Logger.Error(ex, "Framework handler exception");
                }
            }
        }

        private void Client_Tick(SonarClient source) => Volatile.Write(ref this._tick, true);

        public Task StartAsync(CancellationToken cancellationToken)
        {
            this.Framework.Update += this.Framework_Update;
            this.Client.Tick += this.Client_Tick;
            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            this.Framework.Update -= this.Framework_Update;
            this.Client.Tick -= this.Client_Tick;
            return Task.CompletedTask;
        }

        /// <summary>Triggers every framework update after checks.</summary>
        public event Action<SonarFramework>? Update
        {
            add
            {
                if (value is not null) ImmutableInterlocked.Update(ref this._updateHandlers, (handlers, handler) => handlers.Add(handler), value);
            }
            remove
            {
                if (value is not null) ImmutableInterlocked.Update(ref this._updateHandlers, (handlers, handler) => handlers.Remove(handler), value);
            }
        }

        /// <summary>Triggers every framework update after checks, but only once per Sonar tick (400ms).</summary>
        public event Action<SonarFramework>? Tick
        {
            add
            {
                if (value is not null) ImmutableInterlocked.Update(ref this._tickHandlers, (handlers, handler) => handlers.Add(handler), value);
            }
            remove
            {
                if (value is not null) ImmutableInterlocked.Update(ref this._tickHandlers, (handlers, handler) => handlers.Remove(handler), value);
            }
        }
    }
}
