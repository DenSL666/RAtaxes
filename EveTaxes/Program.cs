using EveCommon;
using EveCommon.Interfaces;
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
using Microsoft.Extensions.Logging;
using NLog;
using StaticDataStorage.Workers;
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
        const string UpdateMineralArg = "updatemineral";
        /// <summary>
        /// Параметр запуска программы для составления отчета.
        /// </summary>
        const string ReportArg = "report";
        /// <summary>
        /// Параметр запуска программы для составления текстового файла для гугл таблицы.
        /// </summary>
        const string GoogleSdeArg = "googlesde";
        /// <summary>
        /// Параметр запуска программы для составления текстового файла для гугл таблицы.
        /// </summary>
        const string UpdateSdeArg = "updatesde";
        /// <summary>
        /// Массив параметров запуска программы, допустимых для автоматического запуска (без среды разработки).
        /// </summary>
        static readonly string[] ARGS = [UpdateArg, UpdateMineralArg, ReportArg, GoogleSdeArg, UpdateSdeArg];

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
                DeleteLogFiles();

                services.AddSingleton<IFileService, FileService>();
                services.AddSingleton<IDownloadManager, DownloadManager>();

                services.AddHttpClient("DefaultClient")
                    .ConfigureHttpClient(client =>
                    {
                        client.Timeout = TimeSpan.FromSeconds(30);
                        client.DefaultRequestHeaders.Add("Accept", "application/json");
                    })
                    .ConfigurePrimaryHttpMessageHandler(() => new SocketsHttpHandler
                    {
                        PooledConnectionLifetime = TimeSpan.FromMinutes(5),
                        UseProxy = false,
                        AutomaticDecompression = System.Net.DecompressionMethods.GZip
                    });

                services.AddHttpClient("DownloadClient")
                    .ConfigureHttpClient(client =>
                    {
                        client.Timeout = TimeSpan.FromMinutes(30); // Длинный таймаут для больших файлов
                        //client.DefaultRequestHeaders.Add("User-Agent", "YourApp-Downloader/1.0");
                    })
                    .ConfigurePrimaryHttpMessageHandler(() => new SocketsHttpHandler
                    {
                        PooledConnectionLifetime = TimeSpan.FromMinutes(30),
                        UseProxy = true,
                        Proxy = null, // Или настройте прокси если нужно
                        AutomaticDecompression = System.Net.DecompressionMethods.GZip |
                                               System.Net.DecompressionMethods.Deflate,
                        MaxConnectionsPerServer = 10,
                        // Увеличиваем буферы для скачивания файлов
                        MaxResponseHeadersLength = 64, // KB
                        // Настройки для Keep-Alive
                        KeepAlivePingDelay = TimeSpan.FromSeconds(30),
                        KeepAlivePingTimeout = TimeSpan.FromSeconds(5),
                        KeepAlivePingPolicy = HttpKeepAlivePingPolicy.Always
                    });

                services.AddHttpClient<OAuthHelper>();
                services.AddHttpClient<EsiHelper>();
                services.AddHttpClient<WebHelper>();

                //  Добавляем в сервисы класс с форматом создания отдельного экземпляра при каждом запросе.
                services.AddScoped<OAuthHelper>(sp =>
                {
                    var httpClientFactory = sp.GetRequiredService<IHttpClientFactory>();
                    var httpClient = httpClientFactory.CreateClient("DefaultClient");
                    var config = sp.GetRequiredService<IConfig>();
                    var logger = sp.GetRequiredService<ILogger<OAuthHelper>>();
                    return new OAuthHelper(httpClient, config, logger);
                });
                services.AddScoped<EsiHelper>(sp =>
                {
                    var httpClientFactory = sp.GetRequiredService<IHttpClientFactory>();
                    var httpClient = httpClientFactory.CreateClient("DefaultClient");
                    var config = sp.GetRequiredService<IConfig>();
                    var logger = sp.GetRequiredService<ILogger<EsiHelper>>();
                    return new EsiHelper(httpClient, config, logger);
                });
                services.AddScoped<WebHelper>(sp =>
                {
                    var httpClientFactory = sp.GetRequiredService<IHttpClientFactory>();
                    var httpClient = httpClientFactory.CreateClient("DefaultClient");
                    var config = sp.GetRequiredService<IConfig>();
                    var logger = sp.GetRequiredService<ILogger<WebHelper>>();
                    return new WebHelper(httpClient, config, logger);
                });
                services.AddScoped<SdeWebHelper>(sp =>
                {
                    var httpClientFactory = sp.GetRequiredService<IHttpClientFactory>();
                    var httpClient = httpClientFactory.CreateClient("DownloadClient");
                    var config = sp.GetRequiredService<IConfig>();
                    var logger = sp.GetRequiredService<ILogger<SdeWebHelper>>();
                    var downloadManager = sp.GetRequiredService<IDownloadManager>();
                    return new SdeWebHelper(httpClient, config, logger, downloadManager);
                });
                
                services.AddScoped<SdeMain>();
                services.AddScoped<UpdateDataLogic>();
                services.AddScoped<CreateReportLogic>();
                services.AddScoped<UpdateSdeLogic>();
                services.AddSingleton<StaticDataStorageReader>();


                using ServiceProvider provider = services.BuildServiceProvider();
                DIManager.ServiceProvider = provider;

                //  Выполняем миграцию БД при каждом запуске
                StorageContext.Migrate();

                var updateSdeWorker = DIManager.ServiceProvider.GetService<UpdateSdeLogic>();
                await updateSdeWorker.UpdateSdeDataAsync();

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
                            var staticDataStorageReader = DIManager.ServiceProvider.GetService<StaticDataStorageReader>();
                            staticDataStorageReader.CreateBlueprints("items.txt", "bps.txt");
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

        private static void DeleteLogFiles()
        {
            var maxLogFiles = DIManager.Configuration.GetValue<int>("Runtime:MaxLogFiles");
            var logPath = DIManager.Configuration.GetValue<string>("Runtime:PathLog");
            if (Directory.Exists(logPath))
            {
                var files = Directory.GetFiles(logPath, "*.log");

                if (files.Any() && files.Length > maxLogFiles)
                {
                    var deleteFiles = files.Select(x => new FileInfo(x)).OrderByDescending(x => x.CreationTime).Skip(maxLogFiles).ToList();
                    foreach (var file in deleteFiles)
                    {
                        try
                        {
                            file.Delete();
                        }
                        catch { }
                    }
                }
            }
        }
    }
}