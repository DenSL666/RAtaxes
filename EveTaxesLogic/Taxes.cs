using EveCommon.Interfaces;
using EveCommon.Models;
using EveDataStorage.Contexts;
using EveDataStorage.Models;
using EveSdeModel;
using EveSdeModel.Models;
using EveTaxesLogic.Models;
using EveWebClient.SSO;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EveTaxesLogic
{
    public static class Taxes
    {
        public static List<CorporationTax> CalculateCorporations(IReadOnlyCollection<TypeMaterial> oreList, IReadOnlyCollection<ObservedMining> corpLedger, 
            IReadOnlyCollection<CharacterMain> characterMains, IReadOnlyCollection<ItemPrice> prices, IReadOnlyCollection<WalletTransaction> walletTransactions,
            IReadOnlyCollection<MineralMining> mineralMinings, IConfig config)
        {
            //  превращаем лунный леджер в сущность персонажа
            var charactersTaxes = corpLedger.GroupBy(x => x.CharacterId).Where(x => x.ToArray().Any()).Select(x => new CharacterTax(x)).ToList();

            //  обновляем список сущностей и добавляем в них минеральные руды
            charactersTaxes = AddMineralMiningCharacters(charactersTaxes, mineralMinings);

            //  обновляем список сущностей и добавляем в них крабские налоги
            charactersTaxes = AddWalletTransactionCharacters(charactersTaxes, walletTransactions);

            //  делаем расчет налогов
            foreach (var character in charactersTaxes)
            {
                character.CalculateOreTax(oreList, prices, config);
                character.CalculateRatTax(config);
            }

            var userTaxes = new List<UserTax>();
            foreach (var _charTax in charactersTaxes)
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

        #region Вспомогательные методы и селекторы

        public static List<CharacterTax> AddMineralMiningCharacters(List<CharacterTax> characterTaxes, IReadOnlyCollection<IOreModel> mineralMinings)
        {
            var grouped = mineralMinings.GroupBy(x => x.CharacterId).Where(x => x.ToArray().Any());//.Select(x => new CharacterTax(x)).ToList();

            foreach (var group in grouped)
            {
                var charId = group.Key;
                var foundChar = characterTaxes.FirstOrDefault(x => x.CharacterId == charId);
                if (foundChar == null)
                {
                    foundChar = new CharacterTax(group);
                    if (string.IsNullOrEmpty(foundChar.CharacterName) || foundChar.Corporation == null)
                        continue;
                    characterTaxes.Add(foundChar);
                }
                else
                {
                    foundChar.AddOre(group);
                }
            }
            return characterTaxes;
        }

        private static List<CharacterTax> AddWalletTransactionCharacters(List<CharacterTax> characterTaxes, IReadOnlyCollection<WalletTransaction> walletTransactions)
        {
            var dict = walletTransactions.GroupBy(x => x.CharacterId).ToDictionary(x => x.Key, x => x.ToArray());
            foreach (var pair in dict)
            {
                var charId = pair.Key;
                var transactions = pair.Value;

                var foundChar = characterTaxes.FirstOrDefault(x => x.CharacterId == charId);
                if (foundChar == null)
                {
                    foundChar = new CharacterTax(pair);
                    if (string.IsNullOrEmpty(foundChar.CharacterName) || foundChar.Corporation == null)
                        continue;
                    characterTaxes.Add(foundChar);
                }

                foundChar.SetWalletTrasactions(transactions);
            }

            return characterTaxes;
        }

        public static ItemPrice SelectNearestDatePrice(this IReadOnlyCollection<ItemPrice> prices, int typeId, DateTime dateTime)
        {
            if (prices == null || prices.Count == 0)
                return null;

            var pricesById = prices.Where(x => x.TypeId == typeId).ToArray();
            if (!pricesById.Any())
                return null;
            if (pricesById.Length == 1)
                return pricesById[0];

            var result = pricesById.OrderBy(x => Math.Abs((x.DateUpdate - dateTime).Ticks)).First();
            return result;
        }

        public static double SelectPrice(this ItemPrice price, IConfig config)
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

        public static double SelectTax(this TypeMaterial ore, IConfig config)
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

        public static CharacterMain FindMain(this IReadOnlyCollection<CharacterMain> characterMains, int characterId)
        {
            return characterMains.FirstOrDefault(x => x.CharacterId == characterId || x.AssociatedCharacterIds.Contains(characterId));
        }

        public static UserTax FindUser(this IReadOnlyCollection<UserTax> userTaxes, int characterId)
        {
            return userTaxes.FirstOrDefault(x => x.MainCharacterId == characterId || x.AssociatedCharacterIds.Contains(characterId));
        }

        #endregion
    }
}
