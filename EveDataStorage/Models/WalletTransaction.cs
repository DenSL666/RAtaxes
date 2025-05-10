using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EveDataStorage.Models
{
    [Table("WalletTransactions")]
    public class WalletTransaction
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [Column("corporation_id")]
        public int CorporationId { get; set; }

        [Required]
        public int WalletTransactionType { get; set; }

        [Required]
        public long Amount { get; set; }
        [Required]
        public DateTime DateTime { get; set; }
        [Required]
        public int CharacterId { get; set; }

        [NotMapped]
        public Character? Character { get; set; }

        [NotMapped]
        public Corporation? Corporation { get; set; }

        [NotMapped]
        private string _hash;
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
