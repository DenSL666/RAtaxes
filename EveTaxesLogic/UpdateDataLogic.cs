using EveCommon;
using EveCommon.Interfaces;
using EveCommon.Models;
using EveDataStorage.Contexts;
using EveDataStorage.Models;
using EveSdeModel;
using EveWebClient.Esi;
using EveWebClient.Esi.Models;
using EveWebClient.External;
using EveWebClient.External.Models;
using EveWebClient.External.Models.Seat;
using EveWebClient.SSO;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace EveTaxesLogic
{
    public class UpdateDataLogic
    {
        protected IConfiguration Configuration { get; }

        protected IConfig Config { get; }

        protected OAuthHelper AuthHelper { get; }

        protected EsiHelper EsiHelper { get; }

        protected WebHelper WebHelper { get; }

        protected SdeMain SdeMain { get; }

        public UpdateDataLogic (IConfiguration configuration, IConfig config, OAuthHelper authHelper, EsiHelper esiHelper, WebHelper webHelper, SdeMain sdeMain)
        {
            Configuration = configuration;
            Config = config;
            AuthHelper = authHelper;
            EsiHelper = esiHelper;
            WebHelper = webHelper;
            SdeMain = sdeMain;
        }

        public async Task Update(string[] args)
        {
            var _date = Config.LastUpdateDateTime.AddHours(Config.HoursBeforeUpdate);
            if (_date < DateTime.Now)
            {
                StorageContext.CreateBackup();

                var refineIdsStr = SdeMain.AsteroidRefineItems.Select(x => x.Id).ToList();

                var token = await GetAndUpdateToken();

                await SaveCharacterMainsInfo();

                await UpdateCorpMiningStructureLedger(token);

                var esiData = await EsiHelper.ListMarketPricesV1Async();
                MarketPrice.Save(esiData.Model);

                await UpdatePrices(refineIdsStr);

                Config.LastUpdateDateTime = DateTime.Now;
                Config.Write();
            }
        }

        public async Task<AccessTokenDetails> GetAndUpdateToken()
        {
            var path = Configuration.GetValue<string>("Runtime:PathAuth");
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

        private async Task SaveCharacterMainsInfo()
        {
            try
            {
                var users = await WebHelper.GetSeatUserInfo();
                users = users.Where(x => x != null && x.main_character_id > 0).ToList();

                var allCharacterIds = users.Select(x => x.main_character_id).Distinct().ToArray();

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

                            }
                        }
                    }
                    corporationIds = corporationIds.Distinct().ToList();
                    await SaveCorporationsInfo(corporationIds, context);
                }
            }
            catch (Exception exc)
            {

            }
        }

        private async Task UpdateCorpMiningStructureLedger(AccessTokenDetails token)
        {
            var observers = await EsiHelper.CorporationMiningObserversV1Async(token, Config.TaxParams.MiningHoldingCorporationId);
            using (var context = new StorageContext())
            {
                foreach (var observer in observers.Model.OrderBy(x => x.LastUpdated))
                {
                    bool setMaxPage = false;
                    int maxPage = 1, i = 1;
                    do
                    {
                        var observed = await EsiHelper.ObservedCorporationMiningV1Async(token, Config.TaxParams.MiningHoldingCorporationId, observer.ObserverId, i);
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

                            await SaveCharactersInfo(charIds, context);
                            await SaveCorporationsInfo(corpIds, context);

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

        private async Task SaveCorporationsInfo(IEnumerable<int> corpIds, StorageContext context)
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
                var tasks = _corpIds.Select(async x => await EsiHelper.GetCorporationInfoV5Async(x)).ToArray();
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
                        };
                        context.Corporations.Add(corp);
                    }
                }
                context.SaveChanges();
            }
        }

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

        private async Task UpdatePrices(IEnumerable<string> typeIds)
        {
            var currentDateTime = DateTime.Parse(DateTime.Now.ToShortDateString());

            try
            {
                var prices = await WebHelper.GetPrices(typeIds);

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


        private async Task SaveCorporationWallet(int[] allianceIds)
        {
            var corpIds = new List<int>();
            foreach (var id in allianceIds)
            {
                var _res = await EsiHelper.ListAllianceCorporationsV1Async(id);
                corpIds.Add(id);
            }
            corpIds = corpIds.Distinct().Except(Config.TaxParams.CorpIdsToExceptCollectWallet).ToList();


            XmlSerializer serializer = new XmlSerializer(typeof(List<CorpTransact>));
            var corps = new List<CorpTransact>();
            foreach (var corpId in corpIds)
            {
                try
                {
                    int _startPage = await WebHelper.SearchPageSeatCorporationWalletJournal(corpId, DateTime.Parse("2025-01-01"));
                    if (_startPage == -1)
                        continue;

                    var _res = await WebHelper.GetSeatCorporationWalletJournal(corpId, _startPage);
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
