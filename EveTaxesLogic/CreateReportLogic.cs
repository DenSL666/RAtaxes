using EveCommon.Interfaces;
using EveCommon.Models;
using EveDataStorage.Contexts;
using EveDataStorage.Models;
using EveSdeModel;
using EveSdeModel.Models;
using EveWebClient.Esi;
using Microsoft.Extensions.Configuration;
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
            //using (var context = new StorageContext())
            //{
            //    var consts = context.Constellations.Where(x => x.RegionId == 10000014 || x.RegionId == 10000047).ToList();
            //    var constaIds = consts.Select(x => x.Id).ToList();
            //    var solarSystems = context.SolarSystems.Where(x => constaIds.Contains(x.ConstellationId)).ToList();
            //    var solarIds = solarSystems.Select(x => x.Id.ToString()).ToArray();
            //    File.WriteAllLines(@"F:\solars.txt", solarIds);
            //}

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

            var oreFilter = SdeMain.Asteroid.Where(x => x.IsMineral || x.IsIce).Select(x => x.TypeId).ToArray();

            var ledger = GetLedger(startDate.Value, endDate.Value, alliIds: Config.TaxParams.AllianceIdsToCalcTaxes);
            var charMains = GetCharacterMains(alliIds: Config.TaxParams.AllianceIdsToCalcTaxes);
            var prices = GetPrices(SdeMain.AsteroidRefineItems.Select(x => x.TypeId).ToList()).OrderBy(x => x.DateUpdate).ThenBy(x => x.TypeId).ToList();
            var wallets = GetWalletTransactions(startDate.Value, endDate.Value, alliIds: Config.TaxParams.AllianceIdsToCalcTaxes);
            var mineralMining = GetMineralMiningList(startDate.Value, endDate.Value, 
                alliIds: Config.TaxParams.AllianceIdsToCalcTaxes, 
                regionIds: Config.TaxParams.MineralTaxMiningRegions,
                oreTypeIds: oreFilter);

            var calculated = Taxes.CalculateCorporations(SdeMain.Asteroid, ledger, charMains, prices, wallets, mineralMining, Config);
            calculated = calculated.Where(x => x.TotalIskTax >= 30000000).ToList();

            var directory = Path.Combine(AppContext.BaseDirectory, "report");
            if (!Directory.Exists(directory))
                Directory.CreateDirectory(directory);

            var name = $"taxesReport_{startDate.Value.ToLocalTime():yyyy_MM_dd}_{endDate.Value.ToLocalTime():yyyy_MM_dd}.xlsx";
            var filePath = Path.Combine(directory, name);
            Epplus.Export(filePath, calculated, Config.TaxParams.AllianceIdsToCalcTaxes);
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

        private List<MineralMining> GetMineralMiningList(DateTime start, DateTime end, int[]? corpIds = null, int[]? alliIds = null, int[]? oreTypeIds = null, int[]? regionIds = null)
        {
            var result = new List<MineralMining>();

            using (var context = new StorageContext())
            {
                List<int> solarSystems = null;
                if (regionIds != null)
                {
                    solarSystems = new List<int>();
                    foreach (var regionId in regionIds)
                    {
                        var constellations = context.Constellations.Where(x => x.RegionId == regionId).Select(x => x.Id).Distinct().ToArray();
                        solarSystems.AddRange(context.SolarSystems.Where(x => constellations.Contains(x.ConstellationId)).Select(x => x.Id).Distinct());
                    }
                    solarSystems = solarSystems.Distinct().ToList();
                }

                if ((corpIds == null || !corpIds.Any()) && alliIds != null && alliIds.Any())
                {
                    corpIds = context.Corporations.Where(x => x.AllianceId.HasValue && alliIds.Contains(x.AllianceId.Value)).Select(x => x.CorporationId).ToArray();
                }

                if (corpIds != null && corpIds.Any())
                {
                    result = context.MineralMinings
                        .Where(x => 
                        start <= x.LastUpdated && x.LastUpdated < end 
                        && corpIds.Contains(x.CorporationId) 
                        && (oreTypeIds == null || oreTypeIds.Contains(x.TypeId))
                        && (solarSystems == null || solarSystems.Contains(x.SolarSystemId))).ToList();
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
        /// <summary>
        /// Временный код по сохранению sde в БД
        /// </summary>
        [Obsolete]
        private void SaveUniverseSde()
        {
            var regions = SdeMain.InvItems.Where(x => x.IsRegion).ToList();
            var constellations = SdeMain.InvItems.Where(x => x.IsConstellation).ToList();
            var solarSystems = SdeMain.InvItems.Where(x => x.IsSolarSystem).ToList();

            using (var context = new StorageContext())
            {
                foreach (var region in regions)
                {
                    var id = int.Parse(region.Id);
                    var found = context.Regions.FirstOrDefault(x => x.Id == id);
                    if (found == null)
                    {
                        context.Regions.Add(new Region
                        {
                            Id = id,
                            Name = region.Name,
                        });
                        context.SaveChanges();
                    }
                }

                foreach (var constellation in constellations)
                {
                    var id = int.Parse(constellation.Id);
                    var locId = int.Parse(constellation.LocationID);
                    var found = context.Constellations.FirstOrDefault(x => x.Id == id);
                    if (found == null)
                    {
                        context.Constellations.Add(new Constellation
                        {
                            Id = id,
                            Name = constellation.Name,
                            RegionId = locId,
                        });
                        context.SaveChanges();
                    }
                }

                foreach (var solarSystem in solarSystems)
                {
                    var id = int.Parse(solarSystem.Id);
                    var locId = int.Parse(solarSystem.LocationID);
                    var found = context.SolarSystems.FirstOrDefault(x => x.Id == id);
                    if (found == null)
                    {
                        context.SolarSystems.Add(new SolarSystem
                        {
                            Id = id,
                            Name = solarSystem.Name,
                            ConstellationId = locId,
                        });
                        context.SaveChanges();
                    }
                }
            }
        }
    }
}
