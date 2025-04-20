using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EveDataStorage.Models
{
    [Table("Corporations")]
    public class Corporation
    {
        [Key]
        [Column("corporation_id")]
        public int CorporationId { get; set; }

        [Required]
        public string Name { get; set; }

        public int? AllianceId { get; set; }

        public Alliance? Alliance { get; set; }

        public override string ToString()
        {
            var text = Name;
            if (AllianceId.HasValue && Alliance != null)
                text = $"[{Alliance.Name}] {Name}";
            return text;
        }
    }
}
