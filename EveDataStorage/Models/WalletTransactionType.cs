using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EveDataStorage.Models
{
    [Table("WalletTransactionTypes")]
    public class WalletTransactionType
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string Name { get; set; }

        public override string ToString() => Name;
    }
}
