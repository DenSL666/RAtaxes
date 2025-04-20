using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EveDataStorage.Models
{
    [Table("ItemPrices")]
    public class ItemPrice
    {
        [Key]
        public int Id { get; set; }

        public int TypeId { get; set; }

        public DateTime DateUpdate { get; set; }

        public double AveragePrice { get; set; }
        public double JitaBuyPrice { get; set; }
        public double JitaSellPrice { get; set; }

        [NotMapped]
        public double JitaSplit => (JitaSellPrice + JitaBuyPrice) / 2;
    }
}
