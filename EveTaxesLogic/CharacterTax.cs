using EveDataStorage.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EveTaxesLogic
{
    public class CharacterTax
    {
        public int CharacterId { get; }
        public string CharacterName { get; }
        public Corporation Corporation { get; }

        public Dictionary<int, long> MinedDictionary { get; }

        public Dictionary<int, long> RefinedMaterials { get; }

        public long TotalIskGain { get; set; }
        public long TotalIskTax { get; set; }

        public CharacterTax(IGrouping<int, ObservedMining> group)
        {
            CharacterId = group.Key;
            var mined = group.ToList();
            CharacterName = mined.FirstOrDefault(x => x.Character != null)?.Character?.Name;
            Corporation = mined.FirstOrDefault(x => x.Corporation != null)?.Corporation;

            MinedDictionary = mined.GroupBy(x => x.TypeId).ToDictionary(x => x.Key, x => x.Sum(y => y.Quantity));

            RefinedMaterials = new Dictionary<int, long>();
        }
    }
}
