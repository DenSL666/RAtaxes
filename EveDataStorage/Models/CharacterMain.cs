using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EveDataStorage.Models
{
    /// <summary>
    /// Сущность основного пресонажа.
    /// </summary>
    [Table("CharacterMains")]
    public class CharacterMain
    {
        /// <summary>
        /// Id основного персонажа.
        /// </summary>
        [Key]
        [Column("character_id")]
        public int CharacterId { get; set; }

        /// <summary>
        /// Имя основного персонажа.
        /// </summary>
        [Required]
        public string Name { get; set; }

        /// <summary>
        /// Массив id связанных персонажей.
        /// </summary>
        [Required]
        public int[] AssociatedCharacterIds { get; set; }

        /// <summary>
        /// Id корпорации основного персонажа.
        /// </summary>
        [Required]
        public int CorporationId { get; set; }

        /// <summary>
        /// Id альянса, к которому относится корпорация основного персонажа.
        /// </summary>
        public int? AllianceId { get; set; }

        /// <summary>
        /// Сущность корпорации основного персонажа.
        /// </summary>
        [NotMapped]
        public Corporation? Corporation { get; set; }

        /// <summary>
        /// Сущность альянса основного персонажа.
        /// </summary>
        [NotMapped]
        public Alliance? Alliance { get; set; }

        public override string ToString()
        {
            var sb = new StringBuilder();
            if (AllianceId.HasValue && Alliance != null)
                sb.Append($"[{Alliance.Name}]");

            if (Corporation != null)
                sb.Append($" ({Corporation.Name}) ");

            sb.Append(Name);
            return sb.ToString().Trim();
        }
    }
}
