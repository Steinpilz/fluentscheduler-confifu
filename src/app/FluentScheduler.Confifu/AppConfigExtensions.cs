using Confifu.Abstractions;
using Confifu.Abstractions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;
using System;

namespace FluentScheduler.Confifu
{
    public static class AppConfigExtensions
    {
        public class Config
        {
            IAppConfig appConfig;
            public Registry Registry { get; private set; }
            Action<Registry> RegistryConfig = r => { };

            public void Init(IAppConfig appConfig)
            {
                this.appConfig = appConfig;
                Registry = new GenericRegistry();

                appConfig.AddAppRunnerAfter(() =>
                {
                    var actualRegistry = Registry;
                    RegistryConfig?.Invoke(actualRegistry);

                    var serviceProvider = appConfig.GetServiceProvider();
                    if (serviceProvider != null)
                        JobManager.JobFactory = new ServiceProviderJobFactory(serviceProvider);

                    JobManager.Initialize(actualRegistry);
                });
            }

            public void ConfigureLazy(Action<Registry> registryConfig)
            {
                RegistryConfig += registryConfig;
            }
        }

        public static IAppConfig UseFluentScheduler(this IAppConfig appConfig, Action<Config> configurator = null)
        {
            var config = EnsureConfig(appConfig);
            configurator?.Invoke(config);
            return appConfig;
        }

        [Obsolete("Will be removed in the next majox relese. Use UseFluentScheduler(Action<Config>) method instead")]
        public static IAppConfig UseFluentScheduler(this IAppConfig appConfig, Action<Registry> registryConfig)
            => appConfig.UseFluentScheduler(c =>
            {
                registryConfig(c.Registry);
            });

        public static Registry GetFluentSchedulerRegistry(this IAppConfig appConfig)
            => appConfig.EnsureConfig().Registry;

        static Config EnsureConfig(this IAppConfig appConfig)
            => appConfig.EnsureConfig<Config>("FluentScheduler", c => c.Init(appConfig));

        [Obsolete("Doesn't do anything, use UseFluentSchduler (Action<Config>) method to change Registry")]
        public static IAppConfig SetFluentSchedulerRegistry(this IAppConfig appConfig, Registry registry)
        {
            return appConfig;
        }
    }

    class ServiceProviderJobFactory : IJobFactory
    {
        readonly IServiceProvider serviceProvider;

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
