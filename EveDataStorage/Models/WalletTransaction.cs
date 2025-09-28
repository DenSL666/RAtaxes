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
    /// Сущность транзакции между персонажем и корпорацией.
    /// </summary>
    [Table("WalletTransactions")]
    public class WalletTransaction
    {
        /// <summary>
        /// Id записи в БД.
        /// </summary>
        [Key]
        public int Id { get; set; }

        /// <summary>
        /// Id корпорации получателя.
        /// </summary>
        [Required]
        [Column("corporation_id")]
        public int CorporationId { get; set; }

        /// <summary>
        /// Тип транзакции.
        /// </summary>
        [Required]
        public int WalletTransactionType { get; set; }

        /// <summary>
        /// Количество Isk.
        /// </summary>
        [Required]
        public long Amount { get; set; }

        /// <summary>
        /// Дата и время транзакции.
        /// </summary>
        [Required]
        public DateTime DateTime { get; set; }

        /// <summary>
        /// Id персонажа-плательщика.
        /// </summary>
        [Required]
        public int CharacterId { get; set; }

        /// <summary>
        /// Сущность персонажа.
        /// </summary>
        [NotMapped]
        public Character? Character { get; set; }

        /// <summary>
        /// Сущность корпорации.
        /// </summary>
        [NotMapped]
        public Corporation? Corporation { get; set; }

        [NotMapped]
        private string _hash;

        /// <summary>
        /// Хэш строка, описывающая запись о транзакции.
        /// </summary>
        [NotMapped]
        public string Hash
        {
            get
            {
                if (string.IsNullOrEmpty(_hash))
                    _hash = GetHash(CorporationId, DateTime, WalletTransactionType, CharacterId, Amount);
                return _hash;
            }
        }
        public static string GetHash(int CorporationId, DateTime date, int WalletTransactionType, long CharacterId, long Amount) => 
            string.Join('_', CorporationId.ToString(), date.Ticks.ToString(), WalletTransactionType.ToString(), CharacterId.ToString(), Amount.ToString());

        public override string ToString()
        {
            return $"Corp: {CorporationId} | Char: {CharacterId} | {DateTime} | Amount: {Amount}";
        }
    }
}
