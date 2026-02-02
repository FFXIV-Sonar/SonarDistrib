using AG;
using System;

namespace Sonar.Services
{
    public sealed partial class SonarTickService
    {
        private sealed class TickState : IEquatable<TickState>
        {
            public readonly SonarTickService Ticker;
            public readonly Delegate Handler;
            internal bool _running;
            internal int _delayTicks;

            public TickState(SonarTickService ticker, Delegate handler)
            {
                this.Ticker = ticker;
                this.Handler = handler;
            }

            public static bool Equals(TickState? left, TickState? right)
            {
                if (ReferenceEquals(left, right)) return true;
                if (left is null || right is null) return false;
                return left.Ticker == right.Ticker && left.Handler == right.Handler;
            }

            public bool Equals(TickState? other) => Equals(this, other);

            public override bool Equals(object? obj) => obj is TickState other && Equals(this, other);

            public override int GetHashCode() => SplitHash32.Compute([this.Ticker.GetHashCode(), this.Handler.GetHashCode()]);
        }
    }
}
