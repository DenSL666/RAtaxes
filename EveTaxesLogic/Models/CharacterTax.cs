using EveCommon.Interfaces;
using EveCommon.Models;
using EveDataStorage.Models;
using EveSdeModel.Models;
using EveTaxesLogic;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EveTaxesLogic.Models
{
    public class CharacterTax : BaseTax
    {
        public int CharacterId { get; }
        public string CharacterName { get; }
        public Corporation Corporation { get; }

        public Dictionary<int, long> MinedDictionary_Ids { get; }
        public Dictionary<string, long> MinedDictionary_Names { get; }

        public List<IOreModel> OreDataInfos { get; }
        public ReadOnlyCollection<WalletTransaction> WalletTransactions { get; private set; }

        protected CharacterTax()
        {
            OreDataInfos = new List<IOreModel>();
            WalletTransactions = new ReadOnlyCollection<WalletTransaction>([]);
            MinedDictionary_Ids = new Dictionary<int, long>();
            MinedDictionary_Names = new Dictionary<string, long>();
        }

        public CharacterTax(IGrouping<int, IOreModel> group) : this()
        {
            CharacterId = group.Key;
            var mined = group.ToList();
            CharacterName = mined.FirstOrDefault(x => x.Character != null)?.Character?.Name;
            Corporation = mined.FirstOrDefault(x => x.Corporation != null)?.Corporation;

            AddOre(group);
        }

        public CharacterTax(KeyValuePair<int, WalletTransaction[]> pair) : this()
        {
            CharacterId = pair.Key;
            CharacterName = pair.Value.FirstOrDefault(x => x.Character != null)?.Character?.Name;
            Corporation = pair.Value.FirstOrDefault(x => x.Corporation != null)?.Corporation;
        }

        public override string ToString()
        {
            if (Corporation == null)
                return CharacterName;

            return $"[{Corporation.Name}] {CharacterName}";
        }

        public override void SummTaxes()
        {
            
        }

        public void AddOre(IGrouping<int, IOreModel> group)
        {
            var mined = group.ToList();

            var hashes = new HashSet<string>(OreDataInfos.Select(x => x.Hash));
            foreach (var item in mined)
            {
                if (hashes.Add(item.Hash))
                {
                    OreDataInfos.Add(item);
                }
            }
        }

        public void SetWalletTrasactions(IList<WalletTransaction> transactions)
        {
            WalletTransactions = new ReadOnlyCollection<WalletTransaction>(transactions);
        }

        public void FillMinedNames(IReadOnlyCollection<TypeMaterial> oreList)
        {
            foreach (var pair in MinedDictionary_Ids)
            {
                var typeIdMined = pair.Key;
                var quantityMined = pair.Value;

                var foundOre = oreList.FirstOrDefault(x => x.TypeId == typeIdMined);
                if (foundOre != null)
                {
                    MinedDictionary_Names.Add(foundOre.Entity?.Name.English, quantityMined);
                }
            }
        }

        public void CalculateOreTax(IReadOnlyCollection<TypeMaterial> oreList, IReadOnlyCollection<ItemPrice> prices, IConfig config)
        {
            //  словарь для накопления излишков руды после рефайна
            var excessDict = new Dictionary<int, long>();

            foreach(var ore in OreDataInfos)
            {
                var typeIdMined = ore.TypeId;
                var quantityMined = ore.Quantity;
                var dateTime = ore.LastUpdated;

                if (excessDict.TryGetValue(typeIdMined, out long val))
                {
                    quantityMined += val;
                    excessDict[typeIdMined] = 0;
                }

                var foundOre = oreList.FirstOrDefault(x => x.TypeId == typeIdMined);
                if (foundOre != null)
                {
                    var refined = foundOre.Refine(quantityMined, config.TaxParams.RefineEffincency, out long excessCount);

                    if (!excessDict.TryAdd(typeIdMined, excessCount))
                        excessDict[typeIdMined] += excessCount;

                    //  для текущей руды берём налоговую ставку, цену материалов и считаем налог
                    foreach (var item in refined)
                    {
                        var material = item.Key;
                        var quantity = item.Value;

                        var foundPrice = prices.SelectNearestDatePrice(material.TypeId, dateTime);
                        if (foundPrice != null)
                        {
                            var _value = (long)Math.Round(quantity * foundPrice.SelectPrice(config));
                            var _tax = (long)Math.Round(_value * foundOre.SelectTax(config));

                            if (foundOre.IsMoon)
                            {
                                TotalIskGain_MoonMining += _value;
                                TotalIskTax_MoonMining += _tax;
                            }
                            else
                            {
                                TotalIskGain_MineralMining += _value;
                                TotalIskTax_MineralMining += _tax;
                            }
                        }
                    }
                }
            }

            foreach (var pair in MinedDictionary_Ids)
            {
                var typeIdMined = pair.Key;
                var quantityMined = pair.Value;

                var foundOre = oreList.FirstOrDefault(x => x.TypeId == typeIdMined);
                if (foundOre != null)
                {
                    var refined = foundOre.Refine(quantityMined, config.TaxParams.RefineEffincency, out long excessCount);

                    //  для текущей руды берём налоговую ставку, цену материалов и считаем налог
                    foreach (var item in refined)
                    {
                        var material = item.Key;
                        var quantity = item.Value;

                        var foundPrice = prices.FirstOrDefault(x => x.TypeId == material.TypeId);
                        if (foundPrice != null)
                        {
                            var _value = (long)Math.Round(quantity * foundPrice.SelectPrice(config));
                            var _tax = (long)Math.Round(_value * foundOre.SelectTax(config));

                            if (foundOre.IsMoon)
                            {
                                TotalIskGain_MoonMining += _value;
                                TotalIskTax_MoonMining += _tax;
                            }
                            else
                            {
                                TotalIskGain_MineralMining += _value;
                                TotalIskTax_MineralMining += _tax;
                            }
                        }
                    }
                }
            }


        }

        public void CalculateRatTax(IConfig config)
        {
            if (WalletTransactions != null && WalletTransactions.Any() && Corporation != null && Corporation.TaxRate > 0)
            {
                var _value = WalletTransactions.Sum(x => x.Amount);
                var totalGain = (long)Math.Round((double)_value / Corporation.TaxRate);

                var _tax = (long)Math.Round(totalGain * config.TaxParams.TaxRatting);

                TotalIskGain_Ratting = totalGain;
                TotalIskTax_Ratting = _tax;
            }
        }
    }
}
