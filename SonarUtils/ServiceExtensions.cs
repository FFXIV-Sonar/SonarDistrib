using DryIoc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace SonarUtils
{
    public static class ServiceExtensions
    {
        extension (IServiceProvider services)
        {
            /// <summary>Start all <see cref="IHostedService"/>s while also signaling all <see cref="IHostedLifecycleService"/>s.</summary>
            /// <param name="logger"><see cref="ILogger"/>.</param>
            /// <param name="cancellationToken"><see cref="CancellationToken"/>.</param>
            /// <returns>Awaitable asynchronous <see cref="Task"/>.</returns>
            public async Task StartAllServicesAsync(ILogger? logger = null, CancellationToken cancellationToken = default)
            {
                logger ??= NullLogger.Instance;

                logger.LogInformation("Retreiving services to start");
                var hostedServices = services.GetServices<IHostedService>();
                var lifecycleServices = services.GetServices<IHostedLifecycleService>();

                await Task.WhenAll(lifecycleServices.Select(service => Task.Run(async () =>
                {
                    var name = service.GetType().Name;
                    logger.LogInformation("Starting Hosted Lifecycle Service: {name}", name);
                    try
                    {
                        await service.StartingAsync(cancellationToken).ConfigureAwait(false);
                    }
                    catch (Exception ex)
                    {
                        logger.LogError(ex, "Exception occurred while Starting Lifecycle Hosted Service: {name}", name);
                    }
                }, cancellationToken))).ConfigureAwait(ConfigureAwaitOptions.SuppressThrowing);

                await Task.WhenAll(hostedServices.Select(service => Task.Run(async () =>
                {
                    var name = service.GetType().Name;
                    logger.LogInformation("Starting Hosted Service: {name}", name);
                    try
                    {
                        await service.StartAsync(cancellationToken).ConfigureAwait(false);
                    }
                    catch (Exception ex)
                    {
                        logger.LogError(ex, "Exception occurred while Starting Hosted Service: {name}", name);
                    }
                }, cancellationToken))).ConfigureAwait(ConfigureAwaitOptions.SuppressThrowing);

                await Task.WhenAll(lifecycleServices.Select(service => Task.Run(async () =>
                {
                    var name = service.GetType().Name;
                    logger.LogInformation("Started Hosted Lifecycle Service: {name}", name);
                    try
                    {
                        await service.StartAsync(cancellationToken).ConfigureAwait(false);
                    }
                    catch (Exception ex)
                    {
                        logger.LogError(ex, "Exception occurred while Started Hosted Lifecycle Service: {name}", name);
                    }
                }, cancellationToken))).ConfigureAwait(ConfigureAwaitOptions.SuppressThrowing);

                logger.LogInformation("All services started");
            }

            /// <summary>Stop all <see cref="IHostedService"/>s while also signaling all <see cref="IHostedLifecycleService"/>s.</summary>
            /// <param name="logger"><see cref="ILogger"/>.</param>
            /// <param name="cancellationToken"><see cref="CancellationToken"/>.</param>
            /// <returns>Awaitable asynchronous <see cref="Task"/>.</returns>
            public async Task StopAllServicesAsync(ILogger? logger = null, CancellationToken cancellationToken = default)
            {
                logger ??= NullLogger.Instance;

                logger.LogInformation("Retreiving services to stop");
                var hostedServices = services.GetServices<IHostedService>();
                var lifecycleServices = services.GetServices<IHostedLifecycleService>();

                await Task.WhenAll(lifecycleServices.Select(service => Task.Run(async () =>
                {
                    var name = service.GetType().Name;
                    logger.LogInformation("Stopping Hosted Lifecycle Service: {name}", name);
                    try
                    {
                        await service.StoppingAsync(cancellationToken).ConfigureAwait(false);
                    }
                    catch (Exception ex)
                    {
                        logger.LogError(ex, "Exception occurred while Stopping Lifecycle Hosted Service: {name}", name);
                    }
                }, cancellationToken))).ConfigureAwait(ConfigureAwaitOptions.SuppressThrowing);

                await Task.WhenAll(hostedServices.Select(service => Task.Run(async () =>
                {
                    var name = service.GetType().Name;
                    logger.LogInformation("Stopping Hosted Service: {name}", name);
                    try
                    {
                        await service.StopAsync(cancellationToken).ConfigureAwait(false);
                    }
                    catch (Exception ex)
                    {
                        logger.LogError(ex, "Exception occurred while Stopping Hosted Service: {name}", name);
                    }
                }, cancellationToken))).ConfigureAwait(ConfigureAwaitOptions.SuppressThrowing);

                await Task.WhenAll(lifecycleServices.Select(service => Task.Run(async () =>
                {
                    var name = service.GetType().Name;
                    logger.LogInformation("Stopped Hosted Lifecycle Service: {name}", name);
                    try
                    {
                        await service.StopAsync(cancellationToken).ConfigureAwait(false);
                    }
                    catch (Exception ex)
                    {
                        logger.LogError(ex, "Exception occurred while Stopped Hosted Lifecycle Service: {name}", name);
                    }
                }, cancellationToken))).ConfigureAwait(ConfigureAwaitOptions.SuppressThrowing);

                logger.LogInformation("All services stopped");
            }
        }

        extension (IServiceCollection services)
        {
            // Empty
        }

        extension (Container container)
        {
            /// <summary>Perform debug validation on this <see cref="Container"/>.</summary>
            /// <param name="validationExceptions">Validation exceptions.</param>
            /// <param name="logger"><see cref="ILogger"/>.</param>
            /// <returns>Whether validation passed.</returns>
            public bool PerformDebugValidation([NotNullWhen(false)] out IEnumerable<KeyValuePair<ServiceInfo, ContainerException>>? validationExceptions, ILogger? logger = null)
            {
                logger ??= NullLogger.Instance;

                logger.LogInformation("Registered Services:");
                if (logger.IsEnabled(LogLevel.Debug))
                {
                    foreach (var service in container.GetServiceRegistrations())
                    {
                        logger.LogDebug(" - {service}", service);
                    }
                }


                List<KeyValuePair<ServiceInfo, ContainerException>>? validationExceptionsList = null;
                logger.LogInformation("Validating DryIoC Container");
                var validationErrors = container.Validate();
                foreach (var kvp in validationErrors)
                {
                    var (service, exception) = kvp;
                    if (logger.IsEnabled(LogLevel.Error)) logger.LogError(exception, "Validation exception: {name}\n{details}", service.ServiceType.Name, exception.TryGetDetails(container));
                    (validationExceptionsList ??= []).Add(kvp);
                }
                validationExceptions = validationExceptionsList;
                return validationExceptions is null;
            }
        }
    }
}
