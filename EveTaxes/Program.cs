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
        /// <summary>
        /// Параметр запуска программы для обновления данных в БД.
        /// </summary>
        const string UpdateArg = "update";
        /// <summary>
        /// Параметр запуска программы для обновления данных в БД.
        /// </summary>
        const string UpdateMineralArg = "updateMineral";
        /// <summary>
        /// Параметр запуска программы для составления отчета.
        /// </summary>
        const string ReportArg = "report";
        /// <summary>
        /// Параметр запуска программы для составления текстового файла для гугл таблицы.
        /// </summary>
        const string GoogleSdeArg = "googleSde";
        /// <summary>
        /// Массив параметров запуска программы, допустимых для автоматического запуска (без среды разработки).
        /// </summary>
        static readonly string[] ARGS = [UpdateArg, UpdateMineralArg, ReportArg];

        /// <summary>
        /// Основной метод запуска программы.
        /// </summary>
        /// <param name="args">Параметры запуска программы.</param>
        /// <returns></returns>
        private static async Task Main(string[] args)
        {
            //  Запуск программы без параметров или с неверными параметрами недопустим.
            if (args.Length == 0)
                return;
            var param1 = args[0].Trim('-').ToLower();
            if (!ARGS.Contains(param1))
                return;

            //  Получаени объект NLog для обработи возможных исключений в основном теле программы.
            var logger = LogManager.GetCurrentClassLogger();

            try
            {
                //  Внутри обёртки try-catch инициализируем Dependency Injection сервисы и классы.
                ServiceCollection services = new();
                //  На всякий случай коллекцию сервисов сохраняем в статическую переменную для доступа из любого модуля программы.
                DIManager.Registry(services);

                //  Добавляем в сервисы класс с форматом создания отдельного экземпляра при каждом запросе.
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

                //  Выполняем миграцию и создание бэкапа БД при каждом запуске
                StorageContext.Migrate();

                switch (param1)
                {
                    //  Аргумент запуска программы для получения данных
                    case UpdateArg:
                        {
                            var updateDataLogic = DIManager.ServiceProvider.GetService<UpdateDataLogic>();
                            await updateDataLogic.Update(args);
                            break;
                        }
                    //  Аргумент запуска программы для получения данных о руде из минералов
                    case UpdateMineralArg:
                        {
                            var updateDataLogic = DIManager.ServiceProvider.GetService<UpdateDataLogic>();
                            await updateDataLogic.SaveMineralMiningInfo(@"D:\mining.csv");
                            break;
                        }
                    //  Аргумент запуска для создания отчета о налогах
                    case ReportArg:
                        {
                            var createReportLogic = DIManager.ServiceProvider.GetService<CreateReportLogic>();
                            createReportLogic.CreateReport(args);
                            break;
                        }
                    //  Аргумент создания текстового файла с существующими в игре чертежами и рецептами
                    case GoogleSdeArg:
                        {
                            CreateBlueprints("items.txt", "bps.txt");
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

        private static void CreateBlueprints(string savePathFileTypes, string savePathFileBlueprints)
        {
            var sdeMain = DIManager.ServiceProvider.GetService<SdeMain>();
            sdeMain.InitBlueprints();

            if (!string.IsNullOrEmpty(savePathFileTypes))
            {
                var writeTypes = sdeMain.EntityTypes
                    .Where(x => x.IsPublished && x.Name != null
                                && !x.Name.en.Contains("SKIN")
                                && !x.Name.en.EndsWith("Blueprint")
                                && !x.Name.en.EndsWith("Emblem")
                                && !x.Name.en.EndsWith("Limited")
                                && !x.Name.en.EndsWith("Unlimited"))
                    .Select(x => $"  {x.Id}: \"{x.Name.en.Replace('\"', '\'')}\",").ToList();
                using (var wr = new StreamWriter(savePathFileTypes))
                {
                    foreach (var item in writeTypes)
                    {
                        wr.WriteLine(item);
                    }
                }
            }

            if (!string.IsNullOrEmpty(savePathFileBlueprints))
            {
                //фильтр и вывод блюпринтов
                var hasManu = sdeMain.Blueprints.Where(x => x.HasManufactory && !x.IsFuelBlock).ToList();
                using (var wr = new StreamWriter(savePathFileBlueprints))
                {
                    foreach (var bp in hasManu.OrderBy(x => x.Product.Name.en.Replace("'", "").Replace("’", "")))
                    {
                        wr.WriteLine(bp.Write());
                    }
                }
            }
        }
    }
}