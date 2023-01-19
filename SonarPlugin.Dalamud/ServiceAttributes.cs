using System;
using System.Threading;
using System.Threading.Tasks;

namespace SonarPlugin
{
    [AttributeUsage(AttributeTargets.Class)]
    public class TransientServiceAttribute : Attribute { }

    [AttributeUsage(AttributeTargets.Class)]
    public class ScopedServiceAttribute : Attribute { }

    [AttributeUsage(AttributeTargets.Class)]
    public class SingletonServiceAttribute : Attribute { }

    /// <summary>
    /// Stolen from Microsoft.Extensions.Hosting(.Abstractions?)
    /// </summary>
    public interface IHostedService
    {
        public Task StartAsync(CancellationToken cancellationToken);
        public Task StopAsync(CancellationToken cancellationToken);
    }
}
