using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EveWebClient.External.Models.Seat
{
    public class CorpTransact
    {
        public long CorporationId { get; set; }
        public WTransaction[] Transactions { get; set; }
        public int LastPage { get; set; }

        public CorpTransact()
        {
            Transactions = [];
        }
    }

    public class WTransaction
    {
        public long Amount { get; set; }
        public DateTime DateTime { get; set; }
        public int Division { get; set; }
        public string RefType { get; set; }
        public long IssuerId { get; set; }

        public WTransaction()
        {

        }

        public WTransaction(long amount, DateTime dateTime, int division, string refType, long issuerId)
        {
            Amount = amount;
            DateTime = dateTime;
            Division = division;
            RefType = refType;
            IssuerId = issuerId;
        }

        public WTransaction(WalletTransaction walletTransaction)
        {
            Amount = walletTransaction.amount;
            DateTime = walletTransaction.date;
            Division = walletTransaction.division;
            RefType = walletTransaction.ref_type;
            IssuerId = walletTransaction.second_party.entity_id.Value;
        }
    }

    public class CorporationWalletJournal
    {
        [JsonProperty("data")]
        public WalletTransaction[] WalletTransactions { get; set; }

        [JsonProperty("links")]
        public Links Links { get; set; }

        [JsonProperty("meta")]
        public Meta Meta { get; set; }
    }

    public class WalletTransaction
    {
        public long internal_id { get; set; }
        public long id { get; set; }
        public int division { get; set; }
        public DateTime date { get; set; }
        public string ref_type { get; set; }
        public long amount { get; set; }
        public long balance { get; set; }
        public string reason { get; set; }
        public long? tax_receiver_id { get; set; }
        public long? tax { get; set; }
        public long? context_id { get; set; }
        public string context_id_type { get; set; }
        public string description { get; set; }
        public Party first_party { get; set; }
        public Party second_party { get; set; }
    }

    public class Party
    {
        public long? entity_id { get; set; }
        public string name { get; set; }
        public string category { get; set; }
    }

}
