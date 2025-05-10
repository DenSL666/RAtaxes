using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;

using EveCommon.Models;
using Microsoft.Extensions.Configuration;
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

            services.AddSingleton<IConfig, Config>();
            services.AddSingleton<IHttpClient, GlobalHttpClient>();
        }

        private static IConfiguration LoadConfiguration()
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true);

            return builder.Build();
        }
    }
}
