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
    /// Сущность альянса.
    /// </summary>
    [Table("Alliances")]
    public class Alliance
    {
        /// <summary>
        /// Id альянса.
        /// </summary>
        [Key]
        [Column("alliance_id")]
        public int AllianceId { get; set; }

        /// <summary>
        /// Имя альянса.
        /// </summary>
        [Required]
        public string Name { get; set; }

        public override string ToString()
        {
            return Name;
        }
    }
}
