using EveDataStorage.Contexts;
using EveDataStorage.Models;
using EveSdeModel;
using EveSdeModel.Models;
using EveTaxesLogic.Models;
using EveWebClient.SSO;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EveTaxesLogic
{
    public static class Taxes
    {
        public static List<CorporationTax> CalculateCorporations(IEnumerable<TypeMaterial> oreList, IEnumerable<ObservedMining> corpLedger, IEnumerable<CharacterMain> characterMains, IEnumerable<ItemPrice> prices, Config config)
        {
            var charactersMoonMining = CalculateMoonMiningCharacters(oreList, corpLedger, prices, config);
            var userTaxes = new List<UserTax>();
            foreach (var _charTax in charactersMoonMining)
            {
                //  для каждого персонажа
                //  ищем ранее созданного Пользователя с учетом всех связанных с ним персонажей
                var userTax = userTaxes.FindUser(_charTax.CharacterId);
                if (userTax == null)
                {
                    //  если не нашли Пользователя
                    //  ищем главного персонажа из сеата
                    var foundMain = characterMains.FindMain(_charTax.CharacterId);
                    if (foundMain == null)
                    {
                        //  если не нашли главного персонажа из сеата
                        //  создаем Пользователя на основе текущего персонажа
                        userTax = new UserTax(_charTax);
                    }
                    else
                    {
                        //  нашли главного персонажа из сеата
                        //  создаём Пользователя на основе главного (с добавлением всех связанных с ним)
                        userTax = new UserTax(foundMain);
                    }
                    userTaxes.Add(userTax);
                }
                //  Пользователь создан - добавляем в него налоги персонажа
                userTax.CharacterTaxes.Add(_charTax);
            }
            userTaxes.ForEach(x => x.SummTaxes());
            //  объединяем список Пользователей в список корпораций


            var corpTaxes = userTaxes
            .Where(x => x.Corporation != null)
            .GroupBy(x => x.CorporationId)
            .Select(x =>
            {
                var corp = x.First().Corporation;
                var corpTax = new CorporationTax(corp);
                corpTax.UserTaxes.AddRange(x.ToList());
                return corpTax;
            })
            .ToList();

            corpTaxes.ForEach(x => x.SummTaxes());

            return corpTaxes;
        }

        public static List<CharacterTax> CalculateMoonMiningCharacters(IEnumerable<TypeMaterial> oreList, IEnumerable<ObservedMining> corpLedger, IEnumerable<ItemPrice> prices, Config config)
        {
            var grouped = corpLedger.GroupBy(x => x.CharacterId).Where(x => x.ToArray().Any()).Select(x => new CharacterTax(x)).ToList();

            foreach (var character in grouped)
            {
                CalculateCharacterTax(character, oreList, prices, config);
            }
            return grouped;
        }

        private static void CalculateCharacterTax(CharacterTax characterTax, IEnumerable<TypeMaterial> oreList, IEnumerable<ItemPrice> prices, Config config)
        {
            characterTax.MinedDictionary_Names = characterTax.MinedDictionary_Ids.ToDictionary(x => oreList.FirstOrDefault(y => y.TypeId == x.Key)?.Entity?.Name.English, x => x.Value);
            foreach (var pair in characterTax.MinedDictionary_Ids)
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
                            var _value = (long)Math.Round(quantity * foundPrice.SelectPrice(config));
                            var _tax = (long)Math.Round(_value * foundOre.SelectTax(config));

                            characterTax.TotalIskGain_MoonMining += _value;
                            characterTax.TotalIskTax_MoonMining += _tax;
                        }
                    }

                    //просто так считаем все собранные материалы
                    foreach (var item in refined)
                    {
                        if (!characterTax.RefinedMaterials_Ids.ContainsKey(item.Key.TypeId))
                        {
                            characterTax.RefinedMaterials_Ids.Add(item.Key.TypeId, item.Value);
                        }
                        else
                        {
                            characterTax.RefinedMaterials_Ids[item.Key.TypeId] += item.Value;
                        }

                        if (!characterTax.RefinedMaterials_Names.ContainsKey(item.Key.Name.English))
                        {
                            characterTax.RefinedMaterials_Names.Add(item.Key.Name.English, item.Value);
                        }
                        else
                        {
                            characterTax.RefinedMaterials_Names[item.Key.Name.English] += item.Value;
                        }
                    }
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

        private static CharacterMain FindMain(this IEnumerable<CharacterMain> characterMains, int characterId)
        {
            return characterMains.FirstOrDefault(x => x.CharacterId == characterId || x.AssociatedCharacterIds.Contains(characterId));
        }

        private static UserTax FindUser(this IEnumerable<UserTax> userTaxes, int characterId)
        {
            return userTaxes.FirstOrDefault(x => x.MainCharacterId == characterId || x.AssociatedCharacterIds.Contains(characterId));
        }
    }
}
