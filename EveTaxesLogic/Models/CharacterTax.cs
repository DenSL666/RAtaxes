using EveDataStorage.Models;
using EveSdeModel.Models;
using System;
using System.Collections.Generic;
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
        public Dictionary<string, long> MinedDictionary_Names { get; set; }

        public Dictionary<int, long> RefinedMaterials_Ids { get; }
        public Dictionary<string, long> RefinedMaterials_Names { get; set; }

        protected CharacterTax()
        {
            MinedDictionary_Ids = new Dictionary<int, long>();
            RefinedMaterials_Ids = new Dictionary<int, long>();
            MinedDictionary_Names = new Dictionary<string, long>();
            RefinedMaterials_Names = new Dictionary<string, long>();
        }

        public CharacterTax(IGrouping<int, ObservedMining> group) : this()
        {
            CharacterId = group.Key;
            var mined = group.ToList();
            CharacterName = mined.FirstOrDefault(x => x.Character != null)?.Character?.Name;
            Corporation = mined.FirstOrDefault(x => x.Corporation != null)?.Corporation;

            MinedDictionary_Ids = mined.GroupBy(x => x.TypeId).ToDictionary(x => x.Key, x => x.Sum(y => y.Quantity));
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
    }
}
