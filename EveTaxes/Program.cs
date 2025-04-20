using EveSdeModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EveWebClient.SSO;
using EveWebClient.SSO.Models;
using EveDataStorage.Contexts;
using EveDataStorage.Models;
using EveSdeModel.Models;
using EveWebClient.SSO.Models.External;
using EveWebClient.SSO.Models.Esi;
using System.Collections;
using EveTaxesLogic;

namespace EveTaxes
{
    internal class Program
    {
        const int CorporationId = 98681778;
        static readonly string PathConfig = Path.Combine("sso", "config.xml");
        static readonly string PathAuth = Path.Combine("sso", "authData.cfg");
        static readonly string PathEsiPrices = Path.Combine("data", "esiPrices.json");

        private static async Task Main(string[] args)
        {
            SdeMain.InitializeAll(false);
            var asteroid = SdeMain.TypeMaterials.Where(x => x.IsAsteroid).ToList();
            var refineItems = asteroid.SelectMany(x => x.RefineMaterials.Keys).GroupBy(x => x.Id).Select(x => x.First()).ToList();
            var refineIdsStr = refineItems.Select(x => x.Id).ToList();
            var refineIds = refineItems.Select(x => x.TypeId).ToList();

            var config = Config.Read(PathConfig);
            //config.Write(PathConfig);
            var helper = new OAuthHelper(config);
            var esiHelper = new EsiHelper(helper);

            var token = await GetAndUpdateToken(helper);

            var _date = config.LastUpdateDateTime.AddHours(config.HoursBeforeUpdate);
            if (_date < DateTime.Now)
            {
                UpdateCorpMiningStructureLedger(esiHelper, token);

                var esiData = await esiHelper.ListMarketPricesV1Async();
                MarketPrice.Save(esiData.Model, PathEsiPrices);

                UpdatePrices(refineIdsStr, esiHelper);

                config.LastUpdateDateTime = DateTime.Now;
                config.Write(PathConfig);
            }

            int startYear = 2025, endYear = 2025;
            int startMonth = 3, endMonth = 3;
            var startDate = DateTime.Parse($"01.{startMonth}.{startYear}");
            var endDate = DateTime.Parse($"{DateTime.DaysInMonth(endYear, endMonth)}.{endMonth}.{endYear}").AddDays(1);

            var ledger = GetLedger(startDate, endDate, alliIds: [1220922756, 99009805]);
            var prices = GetPrices(refineIds);

            var calculated = Taxes.Calculate(asteroid, ledger, prices, config);

            Epplus.Export("test.xlsx", calculated);
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

        private static async void UpdateCorpMiningStructureLedger(EsiHelper esiHelper, AccessTokenDetails token)
        {
            var observers = await esiHelper.CorporationMiningObserversV1Async(token, CorporationId);
            using (var context = new StorageContext())
            {
                foreach (var observer in observers.Model.OrderBy(x => x.LastUpdated))
                {
                    var foundObserver = context.Observers.FirstOrDefault(x => x.ObserverId == observer.ObserverId && x.LastUpdated == observer.LastUpdated);
                    if (foundObserver == null)
                    {
                        var _observer = new MiningObserver()
                        {
                            LastUpdated = observer.LastUpdated,
                            ObserverId = observer.ObserverId,
                        };
                        context.Observers.Add(_observer);
                        context.SaveChanges();
                    }

                    bool setMaxPage = false;
                    int maxPage = 1, i = 1;
                    do
                    {
                        var observed = await esiHelper.ObservedCorporationMiningV1Async(token, CorporationId, observer.ObserverId, i);
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
                                var foundObserved = context.ObservedMinings.FirstOrDefault(x => x.CharacterId == _observed.CharacterId && x.LastUpdated == observer.LastUpdated && x.ObserverId == observer.ObserverId);
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
                            }
                            context.SaveChanges();
                        }
                    }
                    while (i < maxPage);
                }
            }
        }

        private static async void UpdatePrices(IEnumerable<string> typeIds, EsiHelper esiHelper)
        {
            var currentDateTime = DateTime.Parse(DateTime.Now.ToShortDateString());

            try
            {
                var prices = await Price.GetPrices(typeIds, esiHelper, PathEsiPrices);

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

                var corpIds = charInfos.Select(x => x.Model.CorporationId).Distinct().ToList();
                await SaveCorporationsInfo(corpIds, context, esiHelper);

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
    }
}