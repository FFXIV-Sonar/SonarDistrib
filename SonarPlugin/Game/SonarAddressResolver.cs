using Dalamud.Game;
using System;
using Dalamud.Logging;
using Dalamud.Memory;

namespace SonarPlugin.Game
{
    [SingletonService]
    public sealed class SonarAddressResolver : BaseAddressResolver
    {
        private SigScanner Scanner { get; }
        private readonly Lazy<IntPtr> _instancePtr;

        public IntPtr Instance => this._instancePtr.Value;

        public SonarAddressResolver(SigScanner scanner)
        {
            this.Scanner = scanner;

            this._instancePtr = new(this.ResolveInstanceAddress);
#if DEBUG
            PluginLog.LogDebug($"Instance address found at: {(ulong)this.Instance:X16}");
            if (this.Instance != IntPtr.Zero)
            {
                PluginLog.LogDebug($"Current Instance: i{MemoryHelper.Read<byte>(this._instancePtr.Value)}");
            }
#endif
        }

        private IntPtr ResolveInstanceAddress()
        {
            // Based on:
            //  var instanceNumberAddress = TargetModuleScanner.GetStaticAddressFromSig("48 8D 0D ?? ?? ?? ?? 0F B7 F0 E8 ?? ?? ?? ?? 8B D8 3B C6", 2) + 0x20;
            //  Marshal.ReadByte(instanceNumberAddress);
            //  https://discord.com/channels/205430339907223552/693223864741920788/925968924556800030

            if (this.Scanner.TryGetStaticAddressFromSig("48 8D 0D ?? ?? ?? ?? 0F B7 F0 E8 ?? ?? ?? ?? 8B D8 3B C6", out var address, 2))
            {
                return address + 0x20;
            }
            return IntPtr.Zero;
        }
    }
}
