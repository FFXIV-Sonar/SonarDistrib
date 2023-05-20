using Sonar.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Sonar
{
    public sealed partial class SonarClient
    {
        private readonly NonBlocking.ConcurrentDictionary<int, Action<SupportResponse>?> _supportCallbacks = new();
        
        /// <summary>Send a support message to Sonar team</summary>
        /// <remarks>Validity checks are performed by calling <see cref="SupportMessage.ThrowIfInvalid"/></remarks>
        public void ContactSupport(SupportMessage support, Action<SupportResponse>? responseCallback = null)
        {
            support.ThrowIfInvalid();
            var random = SonarStatic.Random;
            var spinWait = new SpinWait();
            while (true)
            {
                support.Id = random.Next();
                if (this._supportCallbacks.TryAdd(support.Id, responseCallback)) break;
                spinWait.SpinOnce(); // You must really have bad luck to be stuck in an infinite loop, but at least it won't use 100% of a CPU thread
            }
            if (!this.Connection.SendIfConnected(support))
            {
                this._supportCallbacks.TryRemove(support.Id, out _);
                throw new InvalidOperationException("Not connected");
            }
        }

        private void SupportCallback(SupportResponse response)
        {
            if (this._supportCallbacks.TryRemove(response.Id, out var responseCallback)) responseCallback?.Invoke(response);
        }
    }
}
