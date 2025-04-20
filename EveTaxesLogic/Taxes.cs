using EveSdeModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EveDataStorage.Contexts;
using EveDataStorage.Models;
using EveSdeModel.Models;
using System.Collections;
using EveWebClient.SSO;

namespace EveTaxesLogic
{
    public static class Taxes
    {
        public static List<CharacterTax> Calculate(IEnumerable<TypeMaterial> oreList, IEnumerable<ObservedMining> corpLedger, IEnumerable<ItemPrice> prices, Config config)
        {
            //var found = oreList.FirstOrDefault(x => x.ToString() == "Monazite");
            //if (found != null)
            //{
            //    var refined = found.Refine("1000", config.TaxParams.RefineEffincency);
            //}

            var grouped = corpLedger.GroupBy(x => x.CharacterId).Where(x => x.ToArray().Any()).Select(x => new CharacterTax(x)).ToList();

            foreach (var character in grouped)
            {
                CalculateCharacterTax(character, oreList, prices, config);
            }
            return grouped;
        }

        private static void CalculateCharacterTax(CharacterTax characterTax, IEnumerable<TypeMaterial> oreList, IEnumerable<ItemPrice> prices, Config config)
        {
            foreach (var pair in characterTax.MinedDictionary)
            {
                var typeIdMined = pair.Key;
                var quantityMined = pair.Value;

                var foundOre = oreList.FirstOrDefault(x => x.TypeId == typeIdMined);
                if (foundOre != null)
                {
                    var refined = foundOre.Refine(quantityMined, config.TaxParams.RefineEffincency);

                    //  для текущей руды берём налоговую ставку, цену материалов и считаем налог
                    foreach (var item in refined)
                    {
                        var material = item.Key;
                        var quantity = item.Value;

                        var foundPrice = prices.FirstOrDefault(x => x.TypeId == material.TypeId);
                        if (foundPrice != null)
                        {
                            var _value = (int)Math.Round(quantity * foundPrice.SelectPrice(config));
                            var _tax = (int)Math.Round(_value * foundOre.SelectTax(config));

                            characterTax.TotalIskGain += _value;
                            characterTax.TotalIskTax += _tax;
                        }
                    }

                    //  просто так считаем все собранные материалы
                    //foreach (var item in refined)
                    //{
                    //    if (!characterTax.RefinedMaterials.ContainsKey(item.Key.TypeId))
                    //    {
                    //        characterTax.RefinedMaterials.Add(item.Key.TypeId, item.Value);
                    //    }
                    //    else
                    //    {
                    //        characterTax.RefinedMaterials[item.Key.TypeId] += item.Value;
                    //    }
                    //}
                }
            }


        }

        private static double SelectPrice(this ItemPrice price, Config config)
        {
            switch (config.TaxParams.PriceSource)
            {
                case 0:
                    return price.AveragePrice;
                case 1:
                    return price.JitaSellPrice;
                case 2:
                    return price.JitaBuyPrice;
                case 3:
                    return price.JitaSplit;
                default:
                    return price.AveragePrice;
            }
        }

        private static double SelectTax(this TypeMaterial ore, Config config)
        {
            if (ore == null)
                return 0;
            if (ore.IsIce)
                return config.TaxParams.TaxIce;
            if (ore.IsUbiquitousMoon4)
                return config.TaxParams.TaxMoonR4;
            if (ore.IsCommonMoon8)
                return config.TaxParams.TaxMoonR8;
            if (ore.IsUncommonMoon16)
                return config.TaxParams.TaxMoonR16;
            if (ore.IsRareMoon32)
                return config.TaxParams.TaxMoonR32;
            if (ore.IsExceptionalMoon64)
                return config.TaxParams.TaxMoonR64;
            if (ore.IsMineral)
                return config.TaxParams.TaxMinerals;
            return 0;
        }
    }
}
