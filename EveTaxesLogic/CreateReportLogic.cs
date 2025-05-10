using EveCommon.Interfaces;
using EveCommon.Models;
using EveDataStorage.Contexts;
using EveDataStorage.Models;
using EveSdeModel;
using EveSdeModel.Models;
using EveWebClient.Esi;
using Microsoft.Extensions.Logging;
using NLog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EveTaxesLogic
{
    public class CreateReportLogic(IConfig config, ILogger<CreateReportLogic> logger, SdeMain sdeMain)
    {
        protected IConfig Config { get; } = config;
        protected SdeMain SdeMain { get; } = sdeMain;
        protected ILogger<CreateReportLogic> Logger { get; } = logger;

        public void CreateReport(string[] args)
        {
            DateTime? startDate = null, endDate = null;
            //  -report 03.03.2025 14.06.2025
            var _args = args.Skip(1).ToArray();
            if (_args.Length > 0)
            {
                if (DateTime.TryParse(_args[0], out DateTime parsed))
                {
                    startDate = parsed;
                }
            }

            if (startDate == null)
            {
                startDate = DateTime.Parse($"01.{DateTime.Now.Month}.{DateTime.Now.Year}").ToUniversalTime();
            }

            if (_args.Length > 1)
            {
                if (DateTime.TryParse(_args[1], out DateTime parsed))
                {
                    endDate = parsed;
                }
            }

            if (endDate == null)
            {
                endDate = startDate.Value.AddMonths(1);
            }

            if (endDate < startDate)
            {
                var t = endDate;
                endDate = startDate;
                startDate = t;
            }

            var ledger = GetLedger(startDate.Value, endDate.Value, alliIds: Config.TaxParams.AllianceIdsToCalcTaxes);
            var charMains = GetCharacterMains(alliIds: Config.TaxParams.AllianceIdsToCalcTaxes);
            var prices = GetPrices(SdeMain.AsteroidRefineItems.Select(x => x.TypeId).ToList());
            var wallets = GetWalletTransactions(startDate.Value, endDate.Value, alliIds: Config.TaxParams.AllianceIdsToCalcTaxes);

            var calculated = Taxes.CalculateCorporations(SdeMain.Asteroid, ledger, charMains, prices, wallets, Config);

            var name = $"taxesReport_{startDate.Value.ToLocalTime():yyyy_MM_dd}_{endDate.Value.ToLocalTime():yyyy_MM_dd}.xlsx";
            Epplus.Export(name, calculated);
        }

        private List<ObservedMining> GetLedger(DateTime start, DateTime end, int[]? corpIds = null, int[]? alliIds = null)
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

        private List<CharacterMain> GetCharacterMains(int[]? corpIds = null, int[]? alliIds = null)
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

        private List<ItemPrice> GetPrices(IEnumerable<TypeMaterial> materials) => GetPrices(materials.Select(x => x.TypeId).ToArray());

        private List<ItemPrice> GetPrices(IEnumerable<int> typeIds)
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

        private List<WalletTransaction> GetWalletTransactions(DateTime start, DateTime end, int[]? corpIds = null, int[]? alliIds = null)
        {
            var result = new List<WalletTransaction>();

            using (var context = new StorageContext())
            {
                if ((corpIds == null || !corpIds.Any()) && alliIds != null && alliIds.Any())
                {
                    corpIds = context.Corporations.Where(x => x.AllianceId.HasValue && alliIds.Contains(x.AllianceId.Value)).Select(x => x.CorporationId).ToArray();
                }

                if (corpIds != null && corpIds.Any())
                {
                    result = context.WalletTransactions.Where(x => start <= x.DateTime && x.DateTime < end && corpIds.Contains(x.CorporationId)).ToList();
                }

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
            return result;
        }
    }
}
