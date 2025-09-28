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
    /// Сущность персонажа.
    /// </summary>
    [Table("Characters")]
    public class Character
    {
        /// <summary>
        /// Id персонажа.
        /// </summary>
        [Key]
        [Column("character_id")]
        public int CharacterId { get; set; }

        /// <summary>
        /// Имя персонажа.
        /// </summary>
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
