using EveDataStorage.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EveTaxesLogic.Models
{
    public class UserTax
    {
        public int MainCharacterId { get; }
        public string Name { get; set; }

        public int CorporationId { get; set; }
        public Corporation? Corporation { get; set; }

        public int? AllianceId { get; set; }
        public Alliance? Alliance { get; set; }

        public List<CharacterTax> CharacterTaxes { get; set; }

        public int[] AssociatedCharacterIds { get; set; }

        public long TotalIskGain_MoonMining { get; set; }
        public long TotalIskTax_MoonMining { get; set; }

        protected UserTax()
        {
            CharacterTaxes = new List<CharacterTax>();
            AssociatedCharacterIds = [];
        }

        public UserTax(CharacterMain characterMain) : this()
        {
            MainCharacterId = characterMain.CharacterId;
            Name = characterMain.Name;
            CorporationId = characterMain.CorporationId;
            Corporation = characterMain.Corporation;
            AllianceId = characterMain.AllianceId;
            Alliance = characterMain.Alliance;
            AssociatedCharacterIds = characterMain.AssociatedCharacterIds;
        }

        public UserTax(CharacterTax characterTax) : this()
        {
            MainCharacterId = characterTax.CharacterId;
            Name = characterTax.CharacterName;
            Corporation = characterTax.Corporation;
            if (Corporation != null)
            {
                CorporationId = Corporation.CorporationId;
                if (Corporation.AllianceId.HasValue)
                {
                    AllianceId = Corporation.AllianceId;
                }
                if (Corporation.Alliance != null)
                {
                    Alliance = Corporation.Alliance;
                }
            }
            AssociatedCharacterIds = [characterTax.CharacterId];
        }

        public void SummTaxes()
        {
            TotalIskGain_MoonMining = CharacterTaxes.Sum(x => x.TotalIskGain_MoonMining);
            TotalIskTax_MoonMining = CharacterTaxes.Sum(x => x.TotalIskTax_MoonMining);
        }

        public override string ToString()
        {
            if (Corporation == null)
                return Name;
            if (Alliance == null)
                return $"[{Corporation.Name}] {Name}";
            else
                return $"[{Alliance.Name} {Corporation.Name}] {Name}";
        }
    }
}
