using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EveDataStorage.Models
{
    [Table("Characters")]
    public class Character
    {
        [Key]
        [Column("character_id")]
        public int CharacterId { get; set; }

        [Required]
        public string Name { get; set; }

        //public int CorporationId { get; set; }

        //public int AllianceId { get; set; }

        public override string ToString()
        {
            return Name;
        }
    }
}
