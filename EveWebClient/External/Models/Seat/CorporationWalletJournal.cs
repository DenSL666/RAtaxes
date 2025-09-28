using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EveWebClient.External.Models.Seat
{
    /// <summary>
    /// Транзакции корпорации.
    /// </summary>
    public class CorpTransact
    {
        /// <summary>
        /// Id корпорации.
        /// </summary>
        public int CorporationId { get; set; }
        /// <summary>
        /// Список транзакций.
        /// </summary>
        public WTransaction[] Transactions { get; set; }
        /// <summary>
        /// Последняя обработанная страница транзакций сеата.
        /// </summary>
        public int LastPage { get; set; }

        public CorpTransact()
        {
            Transactions = [];
        }
    }

    /// <summary>
    /// Данные о транзакции.
    /// </summary>
    public class WTransaction
    {
        public long Amount { get; set; }
        public DateTime DateTime { get; set; }
        public int Division { get; set; }
        public string RefType { get; set; }

        /// <summary>
        /// Id плательщика.
        /// </summary>
        public int IssuerId { get; set; }

        public WTransaction()
        {

        }

        public WTransaction(long amount, DateTime dateTime, int division, string refType, int issuerId)
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

    /// <summary>
    /// Обёртка для ответа сеата на запрос списка транзакций.
    /// </summary>
    public class CorporationWalletJournal
    {
        [JsonProperty("data")]
        public WalletTransaction[] WalletTransactions { get; set; }

        [JsonProperty("links")]
        public Links Links { get; set; }

        [JsonProperty("meta")]
        public Meta Meta { get; set; }
    }

    /// <summary>
    /// Данные о транзакции сеата.
    /// </summary>
    public class WalletTransaction
    {
        public long internal_id { get; set; }
        public long id { get; set; }
        /// <summary>
        /// Номер счета корпорации (1-7), на котором проходила транзакция.
        /// </summary>
        public int division { get; set; }
        /// <summary>
        /// Дата и время транзакции.
        /// </summary>
        public DateTime date { get; set; }
        /// <summary>
        /// Тип транзакции (категория сеата о её сути)
        /// </summary>
        public string ref_type { get; set; }
        /// <summary>
        /// Количество исок.
        /// </summary>
        public long amount { get; set; }
        public long balance { get; set; }
        public string reason { get; set; }
        public long? tax_receiver_id { get; set; }
        public long? tax { get; set; }
        public long? context_id { get; set; }
        public string context_id_type { get; set; }
        public string description { get; set; }
        /// <summary>
        /// Получатель.
        /// </summary>
        public Party first_party { get; set; }
        /// <summary>
        /// Плательщик.
        /// </summary>
        public Party second_party { get; set; }
    }

    /// <summary>
    /// Сторона взаимодействия в транзакции.
    /// </summary>
    public class Party
    {
        /// <summary>
        /// Id объекта.
        /// </summary>
        public int? entity_id { get; set; }
        /// <summary>
        /// Имя объекта.
        /// </summary>
        public string name { get; set; }
        /// <summary>
        /// Категория, к которой относится объект.
        /// </summary>
        public string category { get; set; }
    }

}
