using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EveDataStorage.Models
{
    [Table("CharacterMains")]
    public class CharacterMain
    {
        [Key]
        [Column("character_id")]
        public int CharacterId { get; set; }

        [Required]
        public string Name { get; set; }

        [Required]
        public int[] AssociatedCharacterIds { get; set; }

        [Required]
        public int CorporationId { get; set; }

        public int? AllianceId { get; set; }

        [NotMapped]
        public Corporation? Corporation { get; set; }
        [NotMapped]
        public Alliance? Alliance { get; set; }
    }
}
