﻿using System.Collections.Generic;
using Xunit;
using Confifu;
using Confifu.Abstractions;
using FluentScheduler.Confifu;
using FluentScheduler;
using Confifu.ConfigVariables;
using System.Diagnostics;
using System.Threading;

namespace FluentScheduler.Confifu.Tests
{
    public class ConfigTests
    {
        [Fact]
        public void it_does_not_smoke()
        {
            var appConfig = CreateAppConfig();
            appConfig.UseFluentScheduler();
        }

        [Fact]
        public void it_schedules_and_run_task()
        {
            var itIsWorking = false;
            var appConfig = CreateAppConfig();
            appConfig.UseFluentScheduler(c =>
            {
                c.Registry.Schedule(() => {
                    itIsWorking = true;
                }).ToRunNow();
            });
            appConfig.GetAppRunner()?.Invoke();
        
            Thread.Sleep(100);
            Assert.True(itIsWorking);
        }
        
        [Fact]
        public void it_schedules_lazy_and_run_task()
        {
            var itIsWorking = false;
            var appConfig = CreateAppConfig();
            appConfig.UseFluentScheduler(c =>
            {
                c.ConfigureLazy(r => {
                    r.Schedule(() => {
                        itIsWorking = true;
                    }).ToRunNow();
                });
            });
            appConfig.GetAppRunner()?.Invoke();

            Thread.Sleep(100);
            Assert.True(itIsWorking);
        }

        private static AppConfig CreateAppConfig()
        {
            var appConfig = new AppConfig();
            appConfig.SetConfigVariables(new DictionaryConfigVariables(new Dictionary<string, string>()));
            return appConfig;
        }

    }
}