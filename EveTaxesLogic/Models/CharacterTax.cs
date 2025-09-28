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
    /// <summary>
    /// Описывает модель налогов отдельного персонажа.
    /// </summary>
    public class CharacterTax : BaseTax
    {
        /// <summary>
        /// Id персонажа.
        /// </summary>
        public int CharacterId { get; }

        /// <summary>
        /// Имя персонажа.
        /// </summary>
        public string CharacterName { get; }

        /// <summary>
        /// Сущность корпорации персонажа.
        /// </summary>
        public Corporation Corporation { get; }

        /// <summary>
        /// Словарь отладочных данных, который нужно заполнять отдельным методом.<br/>
        /// Ключ - id добытой руды; Value - количество.
        /// </summary>
        public Dictionary<int, long> MinedDictionary_Ids { get; }

        /// <summary>
        /// Словарь отладочных данных, который нужно заполнять отдельным методом.<br/>
        /// Ключ - название добытой руды; Value - количество.
        /// </summary>
        public Dictionary<string, long> MinedDictionary_Names { get; }

        /// <summary>
        /// Коллекция добытой руды (лунной и минеральной), льда, газа.
        /// </summary>
        public List<IOreModel> OreDataInfos { get; }

        /// <summary>
        /// Коллекция налоговых транзакций персонажа.
        /// </summary>
        public ReadOnlyCollection<WalletTransaction> WalletTransactions { get; private set; }

        protected CharacterTax()
        {
            OreDataInfos = new List<IOreModel>();
            WalletTransactions = new ReadOnlyCollection<WalletTransaction>([]);
            MinedDictionary_Ids = new Dictionary<int, long>();
            MinedDictionary_Names = new Dictionary<string, long>();
        }

        /// <summary>
        /// Создаёт сущность налогов персонажа на основе коллекции добытой руды.
        /// </summary>
        /// <param name="group">Коллекция-группировка добытой руды одним персонажем.</param>
        public CharacterTax(IGrouping<int, IOreModel> group) : this()
        {
            CharacterId = group.Key;
            var mined = group.ToList();
            CharacterName = mined.FirstOrDefault(x => x.Character != null)?.Character?.Name;
            Corporation = mined.FirstOrDefault(x => x.Corporation != null)?.Corporation;

            AddOre(group);
        }

        /// <summary>
        /// Создаёт сущность налогов персонажа на основе коллекции транзакций.
        /// </summary>
        /// <param name="pair">Коллекция-группировка транзакций персонажа.</param>
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

        /// <summary>
        /// Добавить в коллекцию добытой руды новые записи при условии уникальности.
        /// </summary>
        /// <param name="group">Коллекция записей добытой руды для добавления.</param>
        public void AddOre(IGrouping<int, IOreModel> group)
        {
            var mined = group.ToList();

            //  составляем хэш-таблицу записей добытой руды
            var hashes = new HashSet<string>(OreDataInfos.Select(x => x.Hash));
            foreach (var item in mined)
            {
                //  если получилось добавить значение в хэш-таблицу
                //  значит запись уникальна и её нужно добавить в коллекцию добытой руды
                if (hashes.Add(item.Hash))
                {
                    OreDataInfos.Add(item);
                }
            }
        }

        /// <summary>
        /// Устанавливает коллекцию транзакций персонажа.
        /// </summary>
        public void SetWalletTrasactions(IList<WalletTransaction> transactions)
        {
            WalletTransactions = new ReadOnlyCollection<WalletTransaction>(transactions);
        }

        /// <summary>
        /// Для словаря <see cref="MinedDictionary_Ids"/> заполняет словарь <see cref="MinedDictionary_Names"/> на основе коллекции сущностей руд.
        /// </summary>
        /// <param name="oreList">Коллекция сущностей руд.</param>
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

        /// <summary>
        /// Выполняет расчет налогов добытой руды.
        /// </summary>
        /// <param name="oreList">Коллекция сущностей, относящихся к группе астероидов (руды, лёд, газы).</param>
        /// <param name="prices">Коллекция цен на продукты переработки астероидов.</param>
        /// <param name="config">Конфиг налогов.</param>
        public void CalculateOreTax(IReadOnlyCollection<TypeMaterial> oreList, IReadOnlyCollection<ItemPrice> prices, IConfig config)
        {
            //  словарь для накопления излишков руды после рефайна
            var excessDict = new Dictionary<int, long>();

            foreach(var ore in OreDataInfos)
            {
                var typeIdMined = ore.TypeId;
                var quantityMined = ore.Quantity;
                var dateTime = ore.LastUpdated;

                //  если в словаре остатков руды есть руда такого типа, то пробуем её добавить в рефайн этой итерации
                if (excessDict.TryGetValue(typeIdMined, out long val))
                {
                    quantityMined += val;
                    excessDict[typeIdMined] = 0;
                }

                var foundOre = oreList.FirstOrDefault(x => x.TypeId == typeIdMined);
                if (foundOre != null)
                {
                    //  выполняем переработку руды с эффективностью, взятой из конфига
                    var refined = foundOre.Refine(quantityMined, config.TaxParams.RefineEffincency, out long excessCount);

                    //  если после переработки останется какая-то руда, добавим её в словарь
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
        }

        /// <summary>
        /// Выполняет расчет налогов с крабства.
        /// </summary>
        /// <param name="config"></param>
        public void CalculateRatTax(IConfig config)
        {
            if (WalletTransactions != null && WalletTransactions.Any() && Corporation != null && Corporation.TaxRate > 0)
            {
                var _value = WalletTransactions.Sum(x => x.Amount);
                //  делим сумму isk, полученных корпорацией на налоговую ставку корпорации - общее число полученных isk персонажем
                var totalGain = (long)Math.Round((double)_value / Corporation.TaxRate);

                //  умножаем общее число на налоговую ставку альянса (из конфига)
                var _tax = (long)Math.Round(totalGain * config.TaxParams.TaxRatting);

                TotalIskGain_Ratting = totalGain;
                TotalIskTax_Ratting = _tax;
            }
        }
    }
}
