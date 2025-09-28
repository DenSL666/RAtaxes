using EveCommon;
using EveCommon.Interfaces;
using EveCommon.Models;
using EveDataStorage.Contexts;
using EveDataStorage.Models;
using EveSdeModel;
using EveSdeModel.Models;
using EveWebClient.Esi;
using EveWebClient.Esi.Models;
using EveWebClient.External;
using EveWebClient.External.Models;
using EveWebClient.External.Models.Seat;
using EveWebClient.SSO;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace EveTaxesLogic
{
    public class UpdateDataLogic(IConfiguration configuration, IConfig config, ILogger<UpdateDataLogic> logger, OAuthHelper authHelper, EsiHelper esiHelper, WebHelper webHelper, SdeMain sdeMain)
    {
        protected IConfiguration Configuration { get; } = configuration;
        protected IConfig Config { get; } = config;
        protected OAuthHelper AuthHelper { get; } = authHelper;
        protected EsiHelper EsiHelper { get; } = esiHelper;
        protected WebHelper WebHelper { get; } = webHelper;
        protected SdeMain SdeMain { get; } = sdeMain;
        protected ILogger<UpdateDataLogic> Logger { get; } = logger;

        public async Task Update(string[] args)
        {
            try
            {
                //  получаем дату последнего обновления и период обновления
                //  вычисляем минимальную дату, подходящую для обновления данных
                //  сравниваем с текущей датой
                var _date = Config.LastUpdateDateTime.AddHours(Config.HoursBeforeUpdate);
                if (_date < DateTime.Now)
                {
                    //  выбираем id всех сущностей, которые получаются в результате переработки руд
                    var refineIdsStr = SdeMain.AsteroidRefineItems.Select(x => x.Id).ToList();

                    //  обновляем токен доступа к esi
                    //  он актуален в течение 5 минут, так что в идеале проверять его корректность на каждом запросе, если они будут идти более 5 минут подряд
                    var token = await GetAndUpdateToken();

                    await SaveCharacterMainsInfo();

                    await UpdateCorpMiningStructureLedger(token);

                    //  получаем коллекцию цен на все предметы игры из EVE ESI и сохраняем в файл
                    var esiData = await EsiHelper.ListMarketPricesV1Async();
                    MarketPrice.Save(esiData.Model);

                    await UpdatePrices(refineIdsStr);

                    var wallets = await GetCorpWalletInfo();
                    await SaveCorpWalletInfo(wallets);

                    //  обновляем дату получения данных
                    Config.LastUpdateDateTime = DateTime.Now;
                    Config.Write();
                }
            }
            catch (Exception exc)
            {
                Logger.LogError(exc, "Ошибка при попытке обновления данных");
            }
        }

        /// <summary>
        /// Выполняет чтение данных текущего токена. Либо выполняет его обновление, либо выполняет запрос на авторизацию.
        /// </summary>
        /// <returns>Токен авторизции SSO персонажа.</returns>
        public async Task<AccessTokenDetails> GetAndUpdateToken()
        {
            var path = Configuration.GetValue<string>("Runtime:PathAuth");
            path = Path.Combine(AppContext.BaseDirectory, path);
            var token = AccessTokenDetails.Read(path);
            if (!token.IsEmpty)
            {
                var isValid = await AuthHelper.IsTokenValid(token);
                if (!isValid)
                {
                    var newToken = await AuthHelper.RefreshTokenAsync(token);
                    if (newToken != null && !newToken.IsEmpty)
                    {
                        newToken.Write(path);
                        token = newToken;
                    }
                }
            }
            else
            {
                var code = await AuthHelper.GetAuthCodeFromSSO();
                if (string.IsNullOrEmpty(code))
                    throw new Exception("Получен пустой код после редиректа SSO");
                token = await AuthHelper.RequestTokenAsync(code);
                if (token != null && !token.IsEmpty)
                {
                    token.Write(path);
                }
            }

            return token;
        }

        /// <summary>
        /// Выполняет запрос к SEAT для получения списка всех пользователей и их персонажей.<br/>
        /// Обновляет информацию в собственной БД о полученных данных.
        /// </summary>
        private async Task SaveCharacterMainsInfo()
        {
            try
            {
                var users = await WebHelper.GetSeatUserInfo();
                users = users.Where(x => x != null && x.main_character_id > 0).ToList();

                //  Для всех пользователей выбираем id их основного персонажа
                var allCharacterIds = users.Select(x => x.main_character_id).Distinct().ToArray();

                //  Получаем публичные данные о главном персонаже.
                var tasks = allCharacterIds.Select(async x => await EsiHelper.GetCharacterPublicInfoV5Async(x)).ToArray();
                var charInfos = await Task.WhenAll(tasks);
                charInfos = charInfos.Where(x => x != null && x.Model != null).ToArray();

                var corporationIds = new List<int>();

                using (var context = new StorageContext())
                {
                    foreach (var userMain in users)
                    {
                        if (userMain == null || userMain.main_character_id <= 0)
                            continue;

                        var characterInfo = charInfos.FirstOrDefault(x => x?.ObjectId == userMain.main_character_id);

                        if (characterInfo != null && characterInfo.Model != null)
                        {
                            var _data = characterInfo.Model;

                            try
                            {
                                var foundUser = context.CharacterMains.SingleOrDefault(x => x.CharacterId == userMain.main_character_id);
                                if (foundUser == null)
                                {
                                    //  такого мейна еще не было, создаём
                                    var _charMain = new CharacterMain()
                                    {
                                        CharacterId = userMain.main_character_id,
                                        Name = _data.Name,
                                        CorporationId = _data.CorporationId,
                                        AssociatedCharacterIds = userMain.associated_character_ids,
                                    };

                                    if (_data.AllianceId.HasValue)
                                        _charMain.AllianceId = _data.AllianceId.Value;

                                    context.CharacterMains.Add(_charMain);

                                    corporationIds.Add(_data.CorporationId);
                                }
                                else
                                {
                                    //  мейн найден, обновляем его данные
                                    foundUser.CorporationId = _data.CorporationId;
                                    foundUser.AssociatedCharacterIds = userMain.associated_character_ids;

                                    corporationIds.Add(_data.CorporationId);

                                    if (_data.AllianceId.HasValue)
                                        foundUser.AllianceId = _data.AllianceId.Value;
                                }
                                context.SaveChanges();
                            }
                            catch (Exception ex)
                            {
                                Logger.LogError(ex, $"Ошибка при обработке пользователя {userMain.main_character_id}");
                            }
                        }
                    }
                    //  При обработке персонажей запоминали id корпораций
                    corporationIds = corporationIds.Distinct().ToList();
                    //  Обновляем публичные данные корпораций
                    await SaveCorporationsInfo(corporationIds, context);
                }
            }
            catch (Exception exc)
            {
                Logger.LogError(exc, $"Ошибка при обновлении данных пользователей из SEAT.");
            }
        }

        /// <summary>
        /// Выполняет запрос майнинг леджера корпорации и обновление данных в собственной БД.
        /// </summary>
        /// <param name="token">Токен EVE ESI, используемый при запросе данных.</param>
        private async Task UpdateCorpMiningStructureLedger(AccessTokenDetails token)
        {
            var observers = await EsiHelper.CorporationMiningObserversV1Async(token, Config.TaxParams.MiningHoldingCorporationId);
            if (observers == null)
                return;
            using (var context = new StorageContext())
            {
                //  все ранее записанные в БД данные преобразуем в хэш таблицу, чтобы находить совпадения
                var hashes = context.ObservedMinings.Select(x => x.Hash).ToHashSet();

                //  для всех observer (отдельно взятая структура атанор/татара с майнинг леджерами)
                foreach (var observer in observers.Model.Where(x => x != null).OrderBy(x => x.LastUpdated))
                {
                    bool setMaxPage = false;
                    int maxPage = 1, i = 1;
                    do
                    {
                        //  в цикле постранично запрашиваем леджеры структуры
                        //  хотя обычно там всего одна страница
                        var observed = await EsiHelper.ObservedCorporationMiningV1Async(token, Config.TaxParams.MiningHoldingCorporationId, observer.ObserverId, i);
                        i++;
                        if (observed != null)
                        {
                            if (!setMaxPage)
                            {
                                setMaxPage = true;
                                maxPage = observed.MaxPages;
                            }

                            //  из текущего леджера выбираем id персонажей-копателей и id корпораций-копателей
                            var charIds = observed.Model.Select(x => x.CharacterId).Distinct().ToList();
                            var corpIds = observed.Model.Select(x => x.RecordedCorporationId).Distinct().ToList();

                            //  обновляем информацию в БД и о персонажах и о корпорациях
                            await SaveCharactersInfo(charIds, context);
                            await SaveCorporationsInfo(corpIds, context);

                            //  для каждой записи в леджере выбираем данные
                            foreach (var _observed in observed.Model)
                            {
                                //  id персонажа-копателя, дата выкапывания, id структуры, id добытой руды
                                var _hash = ObservedMining.GetHash(_observed.CharacterId, _observed.LastUpdated, observer.ObserverId, _observed.TypeId);
                                
                                //  если такого хэша нет в таблице
                                if (!hashes.Contains(_hash))
                                {
                                    //  создаём запись в БД
                                    var _obs = new ObservedMining()
                                    {
                                        CharacterId = _observed.CharacterId,
                                        LastUpdated = _observed.LastUpdated,
                                        Quantity = _observed.Quantity,
                                        CorporationId = _observed.RecordedCorporationId,
                                        TypeId = _observed.TypeId,
                                        ObserverId = observer.ObserverId,
                                    };
                                    context.ObservedMinings.Add(_obs);
                                    //  добавляем хэш в таблицу
                                    hashes.Add(_hash);
                                }
                                else
                                {
                                    //  если хэш в таблице есть
                                    var foundObserved = context.ObservedMinings.ToList().FirstOrDefault(x => x.Hash == _hash);
                                    //  находим запись и обновляем количество добытой руды (на случай, если персонаж добывал одну и ту же руду несколько дней и количество увеличилось)
                                    if (foundObserved != null)
                                        foundObserved.Quantity = _observed.Quantity;
                                }
                                context.SaveChanges();
                            }
                        }
                    }
                    while (i <= maxPage);
                }
            }
        }

        /// <summary>
        /// Выполняет запрос и обновление публичных данных персонажей из EVE ESI.
        /// </summary>
        /// <param name="charIds">Коллекция Id персонажей.</param>
        /// <param name="context">Открытый контекст БД для обновления данных.</param>
        private async Task SaveCharactersInfo(IEnumerable<int> charIds, StorageContext context)
        {
            var except = new List<int>();
            foreach (var id in charIds)
            {
                var found = context.Characters.FirstOrDefault(x => x.CharacterId == id);
                if (found != null)
                    except.Add(id);
            }
            var _charIds = charIds.Except(except).ToArray();

            if (_charIds.Any())
            {
                var tasks = _charIds.Select(async x => await EsiHelper.GetCharacterPublicInfoV5Async(x)).ToArray();
                var charInfos = await Task.WhenAll(tasks);
                charInfos = charInfos.Where(x => x != null).ToArray();

                foreach (var info in charInfos)
                {
                    var found = context.Characters.FirstOrDefault(x => x.CharacterId == info.ObjectId);
                    if (found == null)
                    {
                        var _char = new Character()
                        {
                            CharacterId = info.ObjectId,
                            Name = info.Model.Name,
                        };
                        context.Characters.Add(_char);
                    }
                }
                context.SaveChanges();
            }
        }

        /// <summary>
        /// Выполняет запрос и обновление публичных данных корпораций из EVE ESI.<br/>
        /// Выполняет запрос и обновление публичных данных альянсов из EVE ESI.
        /// </summary>
        /// <param name="corpIds">Коллекция Id корпораций.</param>
        /// <param name="context">Открытый контекст БД для обновления данных.</param>
        private async Task SaveCorporationsInfo(IEnumerable<int> corpIds, StorageContext context)
        {
            if (corpIds.Any())
            {
                var tasks = corpIds.Select(async x => await EsiHelper.GetCorporationInfoV5Async(x)).ToArray();
                var corpInfos = await Task.WhenAll(tasks);
                corpInfos = corpInfos.Where(x => x != null).ToArray();

                var allianceIds = corpInfos.Where(x => x.Model.AllianceId.HasValue).Select(x => x.Model.AllianceId.Value).Distinct().ToList();
                await SaveAlliancesInfo(allianceIds, context);

                foreach (var info in corpInfos)
                {
                    var found = context.Corporations.FirstOrDefault(x => x.CorporationId == info.ObjectId);
                    if (found == null)
                    {
                        var corp = new Corporation()
                        {
                            CorporationId = info.ObjectId,
                            AllianceId = info.Model.AllianceId,
                            Name = info.Model.Name,
                            TaxRate = info.Model.TaxRate,
                        };
                        context.Corporations.Add(corp);
                    }
                    else
                    {
                        found.AllianceId = info.Model.AllianceId;
                        found.TaxRate = info.Model.TaxRate;
                    }
                }
                context.SaveChanges();
            }
        }

        /// <summary>
        /// Выполняет запрос и обновление публичных данных альянсов из EVE ESI.
        /// </summary>
        /// <param name="allianceIds">Коллекция id альянсов.</param>
        /// <param name="context">Открытый контекст БД для обновления данных.</param>
        private async Task SaveAlliancesInfo(IEnumerable<int> allianceIds, StorageContext context)
        {
            var except = new List<int>();
            foreach (var id in allianceIds)
            {
                var found = context.Alliances.FirstOrDefault(x => x.AllianceId == id);
                if (found != null)
                    except.Add(id);
            }
            var _allianceIds = allianceIds.Except(except).ToArray();

            if (_allianceIds.Any())
            {
                var tasks = _allianceIds.Select(async x => await EsiHelper.GetAllianceInfoV3Async(x)).ToArray();
                var allianceInfos = await Task.WhenAll(tasks);
                allianceInfos = allianceInfos.Where(x => x != null).ToArray();

                foreach (var info in allianceInfos)
                {
                    var found = context.Alliances.FirstOrDefault(x => x.AllianceId == info.ObjectId);
                    if (found == null)
                    {
                        var alliance = new Alliance()
                        {
                            AllianceId = info.ObjectId,
                            Name = info.Model.Name,
                        };
                        context.Alliances.Add(alliance);
                    }
                }
                context.SaveChanges();
            }
        }

        /// <summary>
        /// Получает данные fuzzwork и данные EVE ESI из файла для всех продуктов переработки руд и сохраняет их в БД на текущую дату.
        /// </summary>
        /// <param name="typeIds">Коллекция id продуктов переработки руд.</param>
        private async Task UpdatePrices(IEnumerable<string> typeIds)
        {
            //  выбираем текущую дату
            var currentDateTime = DateTime.Parse(DateTime.Now.ToShortDateString());

            try
            {
                var prices = await WebHelper.GetPrices(typeIds);

                using (var context = new StorageContext())
                {
                    foreach (var item in prices)
                    {
                        var found = context.Prices.FirstOrDefault(x => x.TypeId == item.TypeId && x.DateUpdate == currentDateTime);
                        //  если в БД нет записи о сущности на текущую дату
                        //  создаём запись в БД
                        if (found == null)
                        {
                            var _price = new ItemPrice
                            {
                                TypeId = item.TypeId,
                                DateUpdate = currentDateTime,
                                JitaBuyPrice = item.JitaBuy,
                                JitaSellPrice = item.JitaSell,
                                AveragePrice = item.EveAverage,
                            };
                            context.Prices.Add(_price);
                        }
                        // иначе обновляем цены
                        else
                        {
                            found.JitaBuyPrice = item.JitaBuy;
                            found.JitaSellPrice = item.JitaSell;
                            found.AveragePrice = item.EveAverage;
                        }
                    }
                    context.SaveChanges();
                }
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Ошибка при попытке сохранить данные о ценах");
            }
        }

        /// <summary>
        /// Выполняет запись коллекции данных о транзациях корпораций в БД.
        /// </summary>
        /// <param name="corps">Коллекции данных о транзациях корпораций</param>
        private async Task SaveCorpWalletInfo(IEnumerable<CorpTransact> corps)
        {
            using (var context = new StorageContext())
            {
                //  фильтруем лишь те типы транзакций, которые указаны в конфиге
                var validTypes = context.WalletTransactionTypes.Where(x => Config.TaxParams.CorpTransactTypes.Contains(x.Name)).ToArray();
                //  составляем хэш-таблицу транзакций в БД
                var hashes = context.WalletTransactions.Select(x => x.Hash).ToHashSet();

                //  получаем коллекцию всех id персонажей-плательщиков
                var charIds = corps.SelectMany(x => x.Transactions.Select(y => y.IssuerId)).Distinct().ToArray();
                //  обновляем для них данные в БД
                await SaveCharactersInfo(charIds, context);

                //  получаем коллекцию всех id корпораций
                var corpIds = corps.Select(x => x.CorporationId).Distinct().ToArray();
                await SaveCorporationsInfo(corpIds, context);

                foreach (var corp in corps)
                {
                    //  фильтруем транзакции корпорации по типа из конфига
                    corp.Transactions = corp.Transactions.Where(x => Config.TaxParams.CorpTransactTypes.Contains(x.RefType)).ToArray();

                    //  ищем корпорацию в БД и обновляем последнюю страницу журнала сеата
                    var foundCorp = context.Corporations.FirstOrDefault(x => x.CorporationId == corp.CorporationId);
                    if (foundCorp != null && corp.LastPage > 1)
                    {
                        foundCorp.LastSeatWalletPage = corp.LastPage;
                        context.SaveChanges();
                    }

                    if (foundCorp != null && corp.Transactions.Any())
                    {
                        foreach (var trx in corp.Transactions)
                        {
                            var foundType = validTypes.FirstOrDefault(x => x.Name == trx.RefType);
                            if (foundType != null)
                            {
                                //  если транзакция проходит повсем параметрам, составляем её хэш и сравниваем с хэш-таблицей записей БД
                                var _hash = EveDataStorage.Models.WalletTransaction.GetHash(corp.CorporationId, trx.DateTime, foundType.Id, trx.IssuerId, trx.Amount);
                                if (!hashes.Contains(_hash))
                                {
                                    //  если траназкии нет в БД, добавляем её
                                    var foundWalletTransact = new EveDataStorage.Models.WalletTransaction
                                    {
                                        CorporationId = corp.CorporationId,
                                        WalletTransactionType = foundType.Id,
                                        Amount = trx.Amount,
                                        DateTime = trx.DateTime,
                                        CharacterId = trx.IssuerId,
                                    };
                                    context.WalletTransactions.Add(foundWalletTransact);
                                    context.SaveChanges();
                                    //  и добавляем хэш в таблицу
                                    hashes.Add(_hash);
                                }

                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Выполняет запрос к SEAT для получения данных о транзакциях корпораций.
        /// </summary>
        /// <returns>Коллекция корпораций и их транзакций.</returns>
        private async Task<List<CorpTransact>> GetCorpWalletInfo()
        {
            var result = new List<CorpTransact>();
            //  словарь id корпорации и номер страницы в журнале транзакций корпорации в сеате
            var dict = new Dictionary<int, int>();

            try
            {
                var corpIds = new List<int>();
                //  наполняем список id корпораций из публичных данных альянса
                foreach (var id in Config.TaxParams.AllianceIdsToCalcTaxes)
                {
                    var _res = await EsiHelper.ListAllianceCorporationsV1Async(id);
                    corpIds.AddRange(_res.Model);
                }
                //  исключаем из списка корпорации, для которых не нужно собирать транзакции, согласно конфигу
                corpIds = corpIds.Distinct().Except(Config.TaxParams.CorpIdsToExceptCollectWallet).ToList();

                using (var context = new StorageContext())
                {
                    var notFoundCorpIds = new List<int>();

                    //  для списка корпораций ищем такие, которых нет в нашей БД
                    foreach (var corpId in corpIds)
                    {
                        var foundCorp = context.Corporations.FirstOrDefault(x => x.CorporationId == corpId);
                        if (foundCorp == null)
                        {
                            notFoundCorpIds.Add(corpId);
                        }
                    }

                    //  для не найденных корпораций выполняем поиск данных и запись в БД
                    if (notFoundCorpIds.Count > 0)
                    {
                        await SaveCorporationsInfo(notFoundCorpIds, context);
                    }

                    //  для всех корпораций заполняем словарь страницами журнала сеата
                    foreach (var corpId in corpIds)
                    {
                        var foundCorp = context.Corporations.FirstOrDefault(x => x.CorporationId == corpId);
                        if (foundCorp != null)
                        {
                            //  если страница в БД не заполнена, то будем искать с первой
                            var page = foundCorp.LastSeatWalletPage ?? 1;
                            dict.Add(corpId, page);
                        }
                    }
                }
            }
            catch (Exception exc)
            {
                Logger.LogError(exc, "Ошибка при подготовке словаря для получения данных валлета из сеата");
            }

            //  строковый вид записи текущего месяца
            var curMonth = $"{DateTime.Now.Year}-{DateTime.Now.Month}-01";
            //  отнимаем два месяца, чтобы не анализировать все транзакции в сеате за весь доступный период.
            var twoMonthsBefore = DateTime.Parse(curMonth).AddMonths(-2);

            foreach (var pair in dict)
            {
                //  для каждой корпорации
                var corpId = pair.Key;
                var _startPage = pair.Value;
                try
                {
                    //  если указано выполнять поиск с первой страницы журнала, значит надо попытаться найти более высокую страницу
                    if (_startPage == 1)
                        _startPage = await WebHelper.SearchPageSeatCorporationWalletJournal(corpId, twoMonthsBefore);
                    //  если номер стал отрицательным, значит пропускаем поиск транзакций
                    if (_startPage == -1)
                        continue;

                    //  получаем все доступные транзакции и номер самой последней страницы журнала
                    var _res = await WebHelper.GetSeatCorporationWalletJournal(corpId, _startPage);
                    var _corp = new CorpTransact();
                    _corp.CorporationId = corpId;
                    _corp.LastPage = _res.Key;
                    //  среди транзакций фильтруем лишь те, которые производились с 1 счетом (мастер-счет, куда падают все налоги)
                    //  у которых второй участник это персонаж (чтобы исключить переводы между корпорациями)
                    //  и у которых подходящий тип транзакции, согласно конфигу
                    _corp.Transactions = _res.Value
                        .Where(x => x != null && x.division == 1 && x.second_party != null && x.second_party.entity_id.HasValue && x.second_party.category == "character" && Config.TaxParams.CorpTransactTypes.Contains(x.ref_type))
                        .Select(x => new WTransaction(x))
                        .ToArray();
                    result.Add(_corp);
                }
                catch (Exception exc)
                {
                    Logger.LogError(exc, $"Ошибка при получении данных валлета из сеата для корпорации {corpId} со страницы {_startPage}");
                }
            }

            return result;
        }

        /// <summary>
        /// Выполняет чтение файла экспорта из SEAT майнинг леджера минеральных руд.<br/>
        /// Полученные данные сравнивает с БД и сохраняет.
        /// </summary>
        /// <param name="pathToFile">Путь к файлу на диске.</param>
        /// <returns></returns>
        public async Task SaveMineralMiningInfo(string pathToFile)
        {
            if (!File.Exists(pathToFile))
                throw new FileNotFoundException(pathToFile);
            var minings = new List<MineralMining>();
            var list = new List<string[]>();
            //  читаем в коллекцию массив строковых данных
            using (var rd = new StreamReader(pathToFile))
            {
                while(!rd.EndOfStream)
                {
                    var str = rd.ReadLine();
                    if (!string.IsNullOrEmpty(str))
                    {
                        var arr = str.Split(',').Select(x => x.Trim('\"')).ToArray();
                        if (arr.Length == 7 && int.TryParse(arr[0], out int val))
                        {
                            // charId, solarSys, typeId, quantity, corpId, allianceId, date
                            list.Add(arr);
                        }
                    }
                }
            }

            //  из текущего леджера выбираем id персонажей-копателей и id корпораций-копателей
            var charIds = list.Select(x => int.Parse(x[0])).Distinct().ToArray();
            var corpIds = list.Select(x => int.Parse(x[4])).Distinct().ToArray();

            using (var context = new StorageContext())
            {
                //  составляем хэш-таблицу данных в БД
                var hashes = context.MineralMinings.Select(x => x.Hash).ToHashSet();

                //  обновляем информацию в БД и о персонажах и о корпорациях
                await SaveCharactersInfo(charIds, context);
                await SaveCorporationsInfo(corpIds, context);

                foreach (var arr in list)
                {
                    //  парсим строковые данные
                    var charId = int.Parse(arr[0]);
                    var solarSystemId = int.Parse(arr[1]);
                    var typeId = int.Parse(arr[2]);
                    var quantity = int.Parse(arr[3]);
                    var corpId = int.Parse(arr[4]);
                    var date = DateTime.Parse(arr[6]);

                    //  создаём хэш
                    var _hash = MineralMining.GetHash(charId, date, solarSystemId, typeId, corpId, quantity);

                    //  сравниваем хэш с таблицей
                    if (!hashes.Contains(_hash))
                    {
                        var mineralMining = new MineralMining
                        {
                            CorporationId = corpId,
                            TypeId = typeId,
                            Quantity = quantity,
                            LastUpdated = date,
                            CharacterId = charId,
                            SolarSystemId = solarSystemId,
                        };
                        context.MineralMinings.Add(mineralMining);
                        context.SaveChanges();
                        hashes.Add(_hash);
                    }
                }
            }
        }
    }
}
