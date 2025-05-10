using EveCommon;
using EveCommon.Models;
using EveDataStorage.Contexts;
using EveDataStorage.Models;
using EveSdeModel;
using EveSdeModel.Models;
using EveTaxesLogic;
using EveWebClient.Esi;
using EveWebClient.External;
using EveWebClient.External.Models;
using EveWebClient.External.Models.Seat;
using EveWebClient.SSO;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace EveTaxes
{
    internal class Program
    {
        const string UpdateArg = "update";
        const string ReportArg = "report";
        static readonly string[] ARGS = [UpdateArg, ReportArg];

        private static async Task Main(string[] args)
        {
            if (args.Length == 0)
                return;
            var param1 = args[0].Trim('-').ToLower();
            if (!ARGS.Contains(param1))
                return;

            ServiceCollection services = new();
            DIManager.Registry(services);
            services.AddSingleton<OAuthHelper>();
            services.AddSingleton<EsiHelper>();
            services.AddSingleton<WebHelper>();
            services.AddSingleton<SdeMain>();
            services.AddSingleton<UpdateDataLogic>();
            services.AddSingleton<CreateReportLogic>();

            using ServiceProvider provider = services.BuildServiceProvider();
            DIManager.ServiceProvider = provider;

            StorageContext.Migrate();

            switch (param1)
            {
                case UpdateArg:
                    {
                        var updateDataLogic = DIManager.ServiceProvider.GetService<UpdateDataLogic>();
                        await updateDataLogic.Update(args);
                        break;
                    }
                    ;
                case ReportArg:
                    {
                        var createReportLogic = DIManager.ServiceProvider.GetService<CreateReportLogic>();
                        createReportLogic.CreateReport(args);
                        break;
                    }
            }
        }
    }
}