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
    /// Сущность корпорации.
    /// </summary>
    [Table("Corporations")]
    public class Corporation
    {
        /// <summary>
        /// Id корпорации.
        /// </summary>
        [Key]
        [Column("corporation_id")]
        public int CorporationId { get; set; }

        /// <summary>
        /// Имя корпорации.
        /// </summary>
        [Required]
        public string Name { get; set; }

        /// <summary>
        /// Налоговая ставка Isk корпорации.
        /// </summary>
        public float TaxRate { get; set; }

        /// <summary>
        /// Id альянса, к которому относится корпорация.
        /// </summary>
        public int? AllianceId { get; set; }

        /// <summary>
        /// Сущность альянса.
        /// </summary>
        public Alliance? Alliance { get; set; }

        /// <summary>
        /// Последняя страница журнала транзакций SEAT , откуда было произведено чтение транзакций.
        /// </summary>
        public int? LastSeatWalletPage { get; set; }

        public override string ToString()
        {
            var text = Name;
            if (AllianceId.HasValue && Alliance != null)
                text = $"[{Alliance.Name}] {Name}";
            return text;
        }
    }
}
