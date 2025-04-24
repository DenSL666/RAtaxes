using EveDataStorage.Contexts;
using EveDataStorage.Models;
using EveSdeModel;
using EveSdeModel.Models;
using EveTaxesLogic;
using EveWebClient.External;
using EveWebClient.External.Models;
using EveWebClient.External.Models.Seat;
using EveWebClient.SSO;
using EveWebClient.SSO.Models;
using EveWebClient.SSO.Models.Esi;
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
        static readonly string PathConfig = Path.Combine("sso", "config.xml");
        static readonly string PathAuth = Path.Combine("sso", "authData.cfg");
        static readonly string PathEsiPrices = Path.Combine("data", "esiPrices.json");

        private static async Task Main(string[] args)
        {
            StorageContext.Migrate();

            var config = Config.Read(PathConfig);
            //config.Write(PathConfig);

            if (args.Length == 0)
                return;
            var param1 = args[0].Trim('-').ToLower();

            switch (param1)
            {
                case "update":
                    {
                        await Update(config, args);
                        break;
                    }
                    ;
                case "report":
                    {
                        CreateReport(config, args);
                        break;
                    }
            }
        }

        private static async Task Update(Config config, string[] args)
        {
            var _date = config.LastUpdateDateTime.AddHours(config.HoursBeforeUpdate);
            if (_date < DateTime.Now)
            {
                StorageContext.CreateBackup();

                var helper = new OAuthHelper(config);
                var esiHelper = new EsiHelper(helper);
                var webHelper = new WebHelper(helper, config);

                SdeMain.InitializeAll();
                var refineIdsStr = SdeMain.AsteroidRefineItems.Select(x => x.Id).ToList();

                var token = await GetAndUpdateToken(helper);

                await SaveCharacterMainsInfo(webHelper, esiHelper);

                await UpdateCorpMiningStructureLedger(config, esiHelper, token);

                var esiData = await esiHelper.ListMarketPricesV1Async();
                MarketPrice.Save(esiData.Model, PathEsiPrices);

                UpdatePrices(refineIdsStr, webHelper);

                config.LastUpdateDateTime = DateTime.Now;
                config.Write(PathConfig);
            }
        }

        private static void CreateReport(Config config, string[] args)
        {
            SdeMain.InitializeAll();

            int startYear = 2025, endYear = 2025;
            int startMonth = 3, endMonth = 4;
            var startDate = DateTime.Parse($"01.{startMonth}.{startYear}").ToUniversalTime();
            var endDate = DateTime.Parse($"{DateTime.DaysInMonth(endYear, endMonth)}.{endMonth}.{endYear}").ToUniversalTime().AddDays(1);

            var ledger = GetLedger(startDate, endDate, alliIds: [1220922756, 99009805]);
            var charMains = GetCharacterMains(alliIds: [1220922756, 99009805]);
            var prices = GetPrices(SdeMain.AsteroidRefineItems.Select(x => x.TypeId).ToList());

            var calculated = Taxes.CalculateCorporations(SdeMain.Asteroid, ledger, charMains, prices, config);

            Epplus.Export($"test_v2_{DateTime.Now:yyyy_MM_dd}.xlsx", calculated);
        }


        private static async Task<AccessTokenDetails> GetAndUpdateToken(OAuthHelper helper)
        {
            var token = AccessTokenDetails.Read(PathAuth);
            if (!token.IsEmpty)
            {
                var isValid = await helper.IsTokenValid(token);
                if (!isValid)
                {
                    var newToken = await helper.RefreshTokenAsync(token);
                    if (newToken != null && !newToken.IsEmpty)
                    {
                        newToken.Write(PathAuth);
                        token = newToken;
                    }
                }
            }
            else
            {
                var code = await helper.GetAuthCodeFromSSO();
                if (string.IsNullOrEmpty(code))
                    throw new Exception("Получен пустой код после редиректа SSO");
                token = await helper.RequestTokenAsync(code);
                if (token != null && !token.IsEmpty)
                {
                    token.Write(PathAuth);
                }
            }

            return token;
        }

        private static async Task UpdateCorpMiningStructureLedger(Config config, EsiHelper esiHelper, AccessTokenDetails token)
        {
            var observers = await esiHelper.CorporationMiningObserversV1Async(token, config.TaxParams.MiningHoldingCorporationId);
            using (var context = new StorageContext())
            {
                foreach (var observer in observers.Model.OrderBy(x => x.LastUpdated))
                {
                    bool setMaxPage = false;
                    int maxPage = 1, i = 1;
                    do
                    {
                        var observed = await esiHelper.ObservedCorporationMiningV1Async(token, config.TaxParams.MiningHoldingCorporationId, observer.ObserverId, i);
                        i++;
                        if (observed != null)
                        {
                            if (!setMaxPage)
                            {
                                setMaxPage = true;
                                maxPage = observed.MaxPages;
                            }

                            var charIds = observed.Model.Select(x => x.CharacterId).Distinct().ToList();
                            var corpIds = observed.Model.Select(x => x.RecordedCorporationId).Distinct().ToList();

                            await SaveCharactersInfo(charIds, context, esiHelper);
                            await SaveCorporationsInfo(corpIds, context, esiHelper);

                            foreach (var _observed in observed.Model)
                            {
                                var _hash = ObservedMining.GetHash(_observed.CharacterId, _observed.LastUpdated, observer.ObserverId, _observed.TypeId);
                                var foundObserved = context.ObservedMinings.ToList().FirstOrDefault(x => x.Hash == _hash);
                                if (foundObserved == null)
                                {
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
                                }
                                else
                                {
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

        private static async void UpdatePrices(IEnumerable<string> typeIds, WebHelper webHelper)
        {
            var currentDateTime = DateTime.Parse(DateTime.Now.ToShortDateString());

            try
            {
                var prices = await Price.GetPrices(typeIds, webHelper, PathEsiPrices);

                using (var context = new StorageContext())
                {
                    foreach (var item in prices)
                    {
                        var found = context.Prices.FirstOrDefault(x => x.TypeId == item.TypeId && x.DateUpdate == currentDateTime);
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

            }
        }

        private static async Task SaveCharacterMainsInfo(WebHelper webHelper, EsiHelper esiHelper)
        {
            try
            {
                var users = await webHelper.GetSeatUserInfo();
                users = users.Where(x => x != null && x.main_character_id > 0).ToList();

                var allCharacterIds = users.Select(x => x.main_character_id).Distinct().ToArray();

                var tasks = allCharacterIds.Select(async x => await esiHelper.GetCharacterPublicInfoV5Async(x)).ToArray();
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

                            }
                        }
                    }
                    corporationIds = corporationIds.Distinct().ToList();
                    await SaveCorporationsInfo(corporationIds, context, esiHelper);
                }
            }
            catch (Exception exc)
            {

            }
        }

        private static async Task SaveCharactersInfo(IEnumerable<int> charIds, StorageContext context, EsiHelper esiHelper)
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
                var tasks = _charIds.Select(async x => await esiHelper.GetCharacterPublicInfoV5Async(x)).ToArray();
                var charInfos = await Task.WhenAll(tasks);
                charInfos = charInfos.Where(x => x != null).ToArray();

                //var corpIds = charInfos.Select(x => x.Model.CorporationId).Distinct().ToList();
                //await SaveCorporationsInfo(corpIds, context, esiHelper);

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

        private static async Task SaveCorporationsInfo(IEnumerable<int> corpIds, StorageContext context, EsiHelper esiHelper)
        {
            var except = new List<int>();
            foreach (var id in corpIds)
            {
                var found = context.Corporations.FirstOrDefault(x => x.CorporationId == id);
                if (found != null)
                    except.Add(id);
            }
            var _corpIds = corpIds.Except(except).ToArray();

            if (_corpIds.Any())
            {
                var tasks = _corpIds.Select(async x => await esiHelper.GetCorporationInfoV5Async(x)).ToArray();
                var corpInfos = await Task.WhenAll(tasks);
                corpInfos = corpInfos.Where(x => x != null).ToArray();

                var allianceIds = corpInfos.Where(x => x.Model.AllianceId.HasValue).Select(x => x.Model.AllianceId.Value).Distinct().ToList();
                await SaveAlliancesInfo(allianceIds, context, esiHelper);

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
                        };
                        context.Corporations.Add(corp);
                    }
                }
                context.SaveChanges();
            }
        }

        private static async Task SaveAlliancesInfo(IEnumerable<int> allianceIds, StorageContext context, EsiHelper esiHelper)
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
                var tasks = _allianceIds.Select(async x => await esiHelper.GetAllianceInfoV3Async(x)).ToArray();
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

        private static List<ObservedMining> GetLedger(DateTime start, DateTime end, int[] corpIds = null, int[] alliIds = null)
        {
            var result = new List<ObservedMining>();
            using (var context = new StorageContext())
            {
                result = context.ObservedMinings.Where(x => start <= x.LastUpdated && x.LastUpdated < end).ToList();
                foreach (var line in result)
                {
                    line.Character = context.Characters.FirstOrDefault(x => x.CharacterId == line.CharacterId);
                    line.Corporation = context.Corporations.FirstOrDefault(x => x.CorporationId == line.CorporationId);
                    if (line.Corporation != null)
                    {
                        line.Corporation.Alliance = context.Alliances.FirstOrDefault(x => x.AllianceId == line.Corporation.AllianceId);
                    }
                }
            }

            if (alliIds != null && alliIds.Any())
            {
                result = result.Where(x => x.Corporation != null && x.Corporation.AllianceId.HasValue && alliIds.Contains(x.Corporation.AllianceId.Value)).ToList();
            }

            if (corpIds != null && corpIds.Any())
            {
                result = result.Where(x => corpIds.Contains(x.CorporationId)).ToList();
            }
            result = result.OrderBy(x => x.LastUpdated).ToList();

            return result;
        }

        private static List<CharacterMain> GetCharacterMains(int[] corpIds = null, int[] alliIds = null)
        {
            var result = new List<CharacterMain>();
            using (var context = new StorageContext())
            {
                foreach (var _char in context.CharacterMains)
                {
                    if (corpIds == null || corpIds.Contains(_char.CorporationId))
                    {
                        _char.Corporation = context.Corporations.FirstOrDefault(x => x.CorporationId == _char.CorporationId);
                        if (_char.AllianceId.HasValue)
                        {
                            _char.Alliance = context.Alliances.FirstOrDefault(x => x.AllianceId == _char.AllianceId.Value);
                            _char.Corporation.Alliance = _char.Alliance;
                        }

                        if (alliIds == null || (_char.AllianceId.HasValue && alliIds.Contains(_char.AllianceId.Value)))
                        {
                            result.Add(_char);
                        }
                    }
                }
            }

            return result;
        }

        private static List<ItemPrice> GetPrices(IEnumerable<TypeMaterial> materials) => GetPrices(materials.Select(x => x.TypeId).ToArray());

        private static List<ItemPrice> GetPrices(IEnumerable<int> typeIds)
        {
            var result = new List<ItemPrice>();
            using (var context = new StorageContext())
            {
                if (context.Prices.Any())
                {
                    result = context.Prices.Where(x => typeIds.Contains(x.TypeId)).GroupBy(x => x.TypeId).Select(x => x.OrderByDescending(y => y.DateUpdate).First()).ToList();
                }
            }
            return result;
        }

        private static void RemoveDuplicateEntries_Mining()
        {
            using (var context = new StorageContext())
            {
                var result = context.ObservedMinings.ToList().GroupBy(x => x.Hash).Where(x => x.Count() > 1).ToDictionary(x => x.Key, x => x.ToList());
                foreach (var item in result)
                {
                    var delete = item.Value.OrderBy(x => x.Id).Skip(1).ToList();
                    delete.ForEach(x => context.ObservedMinings.Remove(x));
                    context.SaveChanges();
                }
            }
        }

        private static async Task SaveCorporationWallet(int[] allianceIds, WebHelper webHelper, EsiHelper esiHelper)
        {
            var corpIds = new List<int>();
            foreach (var id in allianceIds)
            {
                var _res = await esiHelper.ListAllianceCorporationsV1Async(id);
                corpIds.Add(id);
            }
            corpIds = corpIds.Distinct().Except([98681778]).ToList();


            XmlSerializer serializer = new XmlSerializer(typeof(List<CorpTransact>));
            var corps = new List<CorpTransact>();
            foreach (var corpId in corpIds)
            {
                try
                {
                    int _startPage = await webHelper.SearchPageSeatCorporationWalletJournal(corpId, DateTime.Parse("2025-01-01"));
                    if (_startPage == -1)
                        continue;

                    var _res = await webHelper.GetSeatCorporationWalletJournal(corpId, _startPage);
                    var _corp = new CorpTransact();
                    _corp.CorporationId = corpId;
                    _corp.LastPage = _res.Key;
                    _corp.Transactions = _res.Value.Where(x => x != null && x.second_party != null && x.second_party.entity_id.HasValue && x.second_party.category == "character").Select(x => new WTransaction(x)).ToArray();
                    corps.Add(_corp);

                    using (Stream writer = new FileStream($"transactions_.xml", FileMode.Create))
                    {
                        serializer.Serialize(writer, corps);
                    }
                }
                catch (Exception ex)
                {

                }
            }
        }
    }
}