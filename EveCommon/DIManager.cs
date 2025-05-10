using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NLog;
using NLog.Extensions.Logging;

using EveCommon.Models;
using EveCommon.Interfaces;

namespace EveCommon
{
    public static class DIManager
    {
        private static IServiceProvider _serviceProvider;
        public static IServiceProvider ServiceProvider
        {
            get => _serviceProvider;
            set
            {
                if (_serviceProvider == null)
                {
                    _serviceProvider = value;
                }
            }
        }

        public static IConfiguration Configuration { get; private set; }

        public static void Registry(IServiceCollection services)
        {
            Configuration = LoadConfiguration();
            services.AddSingleton(Configuration);
            services.AddLogging(loggingBuilder =>
            {
                // configure Logging with NLog
                loggingBuilder.ClearProviders();
                loggingBuilder.SetMinimumLevel(Microsoft.Extensions.Logging.LogLevel.Trace);
                loggingBuilder.AddNLog(Configuration);
            });

            services.AddSingleton<IConfig, Config>();
        }

        private static IConfiguration LoadConfiguration()
        {
            var settingsPath = Path.Combine(AppContext.BaseDirectory, "appsettings.json");
            var builder = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile(settingsPath, optional: true, reloadOnChange: true);

            return builder.Build();
        }
    }
}
