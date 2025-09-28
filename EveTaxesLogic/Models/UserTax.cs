using EveDataStorage.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EveTaxesLogic.Models
{
    /// <summary>
    /// Описывает модель налогов одного пользователя со всеми его связанными персонажами.
    /// </summary>
    public class UserTax : BaseTax
    {
        /// <summary>
        /// Id основного персонажа пользователя.
        /// </summary>
        public int MainCharacterId { get; }

        /// <summary>
        /// Имя основного персонажа пользователя.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Id корпорации основного персонажа пользователя.
        /// </summary>
        public int CorporationId { get; set; }

        /// <summary>
        /// Сущность корпорации основного персонажа пользователя.
        /// </summary>
        public Corporation? Corporation { get; set; }

        /// <summary>
        /// Id альянса основного персонажа пользователя.
        /// </summary>
        public int? AllianceId { get; set; }

        /// <summary>
        /// Сущность альянса основного персонажа пользователя.
        /// </summary>
        public Alliance? Alliance { get; set; }

        /// <summary>
        /// Коллекция налогов связанных персонажей.
        /// </summary>
        public List<CharacterTax> CharacterTaxes { get; set; }

        /// <summary>
        /// Коллекция id связанных персонажей с пользователем.
        /// </summary>
        public int[] AssociatedCharacterIds { get; set; }

        protected UserTax()
        {
            CharacterTaxes = new List<CharacterTax>();
            AssociatedCharacterIds = [];
        }

        /// <summary>
        /// Создает сущность пользователя на основе пользователя SEAT с главным и связанными персонажами.
        /// </summary>
        /// <param name="characterMain">Персонаж.</param>
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

        /// <summary>
        /// Создает сущность пользователя на основе одиночного персонажа.
        /// </summary>
        /// <param name="characterTax">Персонаж.</param>
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

        public override void SummTaxes()
        {
            TotalIskGain_MoonMining = CharacterTaxes.Sum(x => x.TotalIskGain_MoonMining);
            TotalIskTax_MoonMining = CharacterTaxes.Sum(x => x.TotalIskTax_MoonMining);

			TotalIskGain_MineralMining = CharacterTaxes.Sum(x => x.TotalIskGain_MineralMining);
			TotalIskTax_MineralMining = CharacterTaxes.Sum(x => x.TotalIskTax_MineralMining);

			TotalIskGain_Ratting = CharacterTaxes.Sum(x => x.TotalIskGain_Ratting);
            TotalIskTax_Ratting = CharacterTaxes.Sum(x => x.TotalIskTax_Ratting);
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
