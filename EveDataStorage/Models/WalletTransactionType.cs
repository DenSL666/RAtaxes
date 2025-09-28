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
    /// Сущность типа транзакции SEAT.<br/>
    /// (прямой перевод, налог за крабство, налог за есс, налог за миссию...)
    /// </summary>
    [Table("WalletTransactionTypes")]
    public class WalletTransactionType
    {
        /// <summary>
        /// Id записи в БД.
        /// </summary>
        [Key]
        public int Id { get; set; }

        /// <summary>
        /// Тип транзакции.
        /// </summary>
        [Required]
        public string Name { get; set; }

        public override string ToString() => Name;
    }
}
