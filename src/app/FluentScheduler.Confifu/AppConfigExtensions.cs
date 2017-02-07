using Confifu.Abstractions;
using Confifu.Abstractions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;
using System;

namespace FluentScheduler.Confifu
{
    public static class AppConfigExtensions
    {
        public static IAppConfig UseFluentScheduler(this IAppConfig appConfig, Action<Registry> registryConfig)
        {
            var registry = appConfig.GetFluentSchedulerRegistry();
            if(registry == null)
            {
                registry = new GenericRegistry();
                appConfig.SetFluentSchedulerRegistry(registry);
            }

            registryConfig?.Invoke(registry);

            return appConfig.RunOnce("FluentScheduler", () =>
            {
                appConfig.AddAppRunnerAfter(() =>
                {
                    var actualRegistry = appConfig.GetFluentSchedulerRegistry();
                    var serviceProvider = appConfig.GetServiceProvider();
                    if (serviceProvider != null)
                        JobManager.JobFactory = new ServiceProviderJobFactory(serviceProvider);

                    JobManager.Initialize(actualRegistry);
                });
            });
        }

        public static Registry GetFluentSchedulerRegistry(this IAppConfig appConfig)
        {
            return appConfig["FluentScheduler:Registry"] as Registry;
        }

        public static IAppConfig SetFluentSchedulerRegistry(this IAppConfig appConfig, Registry registry)
        {
            appConfig["FluentScheduler:Registry"] = registry;
            return appConfig;
        }
    }

    internal class ServiceProviderJobFactory : IJobFactory
    {
        private readonly IServiceProvider serviceProvider;

        public ServiceProviderJobFactory(IServiceProvider serviceProvider)
        {
            this.serviceProvider = serviceProvider;
        }

        public IJob GetJobInstance<T>() where T : IJob
        {
            return serviceProvider.GetService<T>();
        }
    }
}
