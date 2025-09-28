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
        /// <summary>
        /// Выполняет подсчет всех налоговых сущностей.
        /// </summary>
        /// <param name="oreList">Коллекция сущностей, относящихся к группе астероидов (руды, лёд, газы).</param>
        /// <param name="corpLedger">Коллекция записей добытой лунной руды.</param>
        /// <param name="characterMains">Коллекция ползователей (основных персонажей) и связанных с ними персонажей.</param>
        /// <param name="prices">Коллекция цен на продукты переработки астероидов.</param>
        /// <param name="walletTransactions">Коллекция записей налоговых транзакций корпораций.</param>
        /// <param name="mineralMinings">Коллекция записей добытой минеральной руды.</param>
        /// <param name="config">Конфиг налогов.</param>
        /// <returns>Коллекция налогов по каждой корпорации.</returns>
        public static List<CorporationTax> CalculateCorporations(IReadOnlyCollection<TypeMaterial> oreList, IReadOnlyCollection<ObservedMining> corpLedger, 
            IReadOnlyCollection<CharacterMain> characterMains, IReadOnlyCollection<ItemPrice> prices, IReadOnlyCollection<WalletTransaction> walletTransactions,
            IReadOnlyCollection<MineralMining> mineralMinings, IConfig config)
        {
            // начинаем с лунной руды
            //  группируем все записи по id персонажа
            //  фильтруем по критерию наличия добытой руды (на всякий случай)
            //  создаём на основе записи лунной добычи новую сущность "налоги персонажа"
            var charactersTaxes = corpLedger.GroupBy(x => x.CharacterId).Where(x => x.ToArray().Any()).Select(x => new CharacterTax(x)).ToList();

            //  для ранее созданных сущностей "налоги персонажа" запускает обновление и/или добавление информации о минеральных рудах
            charactersTaxes = AddMineralMiningCharacters(charactersTaxes, mineralMinings);

            //  для ранее созданных сущностей "налоги персонажа" запускает обновление и/или добавление информации о крабских налогах
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
            //  для каждого пользователя подсчитываем налоги
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

            //  для каждой корпорации подсчитываем налоги
            corpTaxes.ForEach(x => x.SummTaxes());

            //  исключаем из списка корпораций те, для которых подсчитывать налоги не нужно
            if (config != null && config.TaxParams != null && config.TaxParams.CorpIdsToExcludeTaxes != null && config.TaxParams.CorpIdsToExcludeTaxes.Any())
            {
                corpTaxes = corpTaxes.Where(x => !config.TaxParams.CorpIdsToExcludeTaxes.Contains(x.CorporationId)).ToList();
            }

            return corpTaxes;
        }

        #region Вспомогательные методы и селекторы

        /// <summary>
        /// Запускает обновление и/или добавление информации о минеральных рудах для коллекции налогов персонажей.
        /// </summary>
        /// <param name="characterTaxes">Коллекция налогов персонажей.</param>
        /// <param name="mineralMinings">Коллекция записей добытой минеральной руды.</param>
        /// <returns>Коллекция налогов персонажей.</returns>
        private static List<CharacterTax> AddMineralMiningCharacters(List<CharacterTax> characterTaxes, IReadOnlyCollection<IOreModel> mineralMinings)
        {
            //  группируем все записи по id персонажа
            //  фильтруем по критерию наличия добытой руды (на всякий случай)
            var grouped = mineralMinings.GroupBy(x => x.CharacterId).Where(x => x.ToArray().Any());

            foreach (var group in grouped)
            {
                var charId = group.Key;
                //  если персонаж не найден в коллекции персонажей
                var foundChar = characterTaxes.FirstOrDefault(x => x.CharacterId == charId);
                if (foundChar == null)
                {
                    //  создаём на основе записи минеральной добычи новую сущность "налоги персонажа"
                    foundChar = new CharacterTax(group);
                    if (string.IsNullOrEmpty(foundChar.CharacterName) || foundChar.Corporation == null)
                        continue;
                    characterTaxes.Add(foundChar);
                }
                //  если найден, то в существующую сущность добавляем минеральные руды
                else
                {
                    foundChar.AddOre(group);
                }
            }
            return characterTaxes;
        }

        /// <summary>
        /// Запускает обновление и/или добавление информации о крабских налогах для коллекции налогов персонажей.
        /// </summary>
        /// <param name="characterTaxes">Коллекция налогов персонажей.</param>
        /// <param name="walletTransactions">Коллекция транзакций персонажа.</param>
        /// <returns>Коллекция налогов персонажей.</returns>
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

        /// <summary>
        /// Выполняет поиск цены сущности из списка по критерию ближайшего к указанной дате и по фильтру id сущности.
        /// </summary>
        /// <param name="prices">Коллекция всех цен всех сущностей за все времена.</param>
        /// <param name="typeId">Id сущности.</param>
        /// <param name="dateTime">Выбранная дата.</param>
        /// <returns>Сущность цены предмета, ближайшая по времени, либо null.</returns>
        public static ItemPrice SelectNearestDatePrice(this IReadOnlyCollection<ItemPrice> prices, int typeId, DateTime dateTime)
        {
            if (prices == null || prices.Count == 0)
                return null;

            var pricesById = prices.Where(x => x.TypeId == typeId).ToArray();
            if (!pricesById.Any())
                return null;
            if (pricesById.Length == 1)
                return pricesById[0];
            //  сортируем по возрастанию все фильтрованные цены по разнице тиков между датой цены и нужной датой, и выбираем первую (с наименьшей разницей)
            var result = pricesById.OrderBy(x => Math.Abs((x.DateUpdate - dateTime).Ticks)).First();
            return result;
        }

        /// <summary>
        /// Выбирает цену на основе заданного конфига.
        /// </summary>
        /// <param name="price">Цены предмета.</param>
        /// <param name="config">Конфиг налогов.</param>
        /// <returns>Выбранная цена, либо по умолчанию средняя по EVE.</returns>
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

        /// <summary>
        /// Выбирает величину налога для указанного типа руды из заданного конфига
        /// </summary>
        /// <param name="ore">Тип руды.</param>
        /// <param name="config">Конфиг налогов.</param>
        /// <returns>Выбранная величина налога или 0.</returns>
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

        /// <summary>
        /// Поиск пользователя в коллекции всех пользователей, для которого найден связанный id персонажа.
        /// </summary>
        /// <param name="characterMains">Коллекции всех пользователей</param>
        /// <param name="characterId">Id персонажа, связанного с каким-то пользователем.</param>
        /// <returns>Найденная сущность пользователя или null.</returns>
        public static CharacterMain FindMain(this IReadOnlyCollection<CharacterMain> characterMains, int characterId)
        {
            return characterMains.FirstOrDefault(x => x.CharacterId == characterId || x.AssociatedCharacterIds.Contains(characterId));
        }

        /// <summary>
        /// Поиск налогов пользователя в коллекции налогов всех пользователей, для которого найден связанный id персонажа.
        /// </summary>
        /// <param name="userTaxes">Коллекции налогов всех пользователей</param>
        /// <param name="characterId">Id персонажа, связанного с каким-то пользователем.</param>
        /// <returns>Найденная сущность налогов пользователя или null.</returns>
        public static UserTax FindUser(this IReadOnlyCollection<UserTax> userTaxes, int characterId)
        {
            return userTaxes.FirstOrDefault(x => x.MainCharacterId == characterId || x.AssociatedCharacterIds.Contains(characterId));
        }

        #endregion
    }
}
