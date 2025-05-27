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
using NLog;
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
        const string GoogleSdeArg = "googleSde";
        static readonly string[] ARGS = [UpdateArg, ReportArg];

        private static async Task Main(string[] args)
        {
            if (args.Length == 0)
                return;
            var param1 = args[0].Trim('-').ToLower();
            if (!ARGS.Contains(param1))
                return;

            var logger = LogManager.GetCurrentClassLogger();

            try
            {
                ServiceCollection services = new();
                DIManager.Registry(services);

                services.AddScoped<OAuthHelper>();
                services.AddScoped<EsiHelper>();
                services.AddScoped<WebHelper>();

                services.AddHttpClient<OAuthHelper>();
                services.AddHttpClient<EsiHelper>();
                services.AddHttpClient<WebHelper>();

                services.AddSingleton<SdeMain>();
                services.AddScoped<UpdateDataLogic>();
                services.AddScoped<CreateReportLogic>();

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
                    case GoogleSdeArg:
                        {
                            var sdeMain = DIManager.ServiceProvider.GetService<SdeMain>();
                            sdeMain.InitBlueprints();
                            {
                                var writeTypes = sdeMain.EntityTypes
                                    .Where(x => x.IsPublished && x.Name != null
                                                && !x.Name.en.Contains("SKIN") 
                                                && !x.Name.en.EndsWith("Blueprint")
                                                && !x.Name.en.EndsWith("Emblem") 
                                                && !x.Name.en.EndsWith("Limited") 
                                                && !x.Name.en.EndsWith("Unlimited"))
                                    .Select(x => $"  {x.Id}: \"{x.Name.en.Replace('\"', '\'')}\",").ToList();
                                using (var wr = new StreamWriter("items.txt"))
                                {
                                    foreach (var item in writeTypes)
                                    {
                                        wr.WriteLine(item);
                                    }
                                }

                                //фильтр и вывод блюпринтов
                                var hasManu = sdeMain.Blueprints.Where(x => x.HasManufactory && !x.IsFuelBlock).ToList();
                                using (var wr = new StreamWriter("bps.txt"))
                                {
                                    foreach (var bp in hasManu.OrderBy(x => x.Product.Name.en.Replace("'", "").Replace("’", "")))
                                    {
                                        wr.WriteLine(bp.Write());
                                    }
                                }
                            }
                            break;
                        }
                }
            }
            catch (Exception exc)
            {
                logger.Error(exc, "Stopped program because of exception");
            }
            finally
            {
                LogManager.Shutdown();
            }
        }
    }
}