using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace EveWebClient.Esi.Models
{
    public class CorporationObservedMining : ModelBase<CorporationObservedMining>
    {
        #region Properties

        /// <summary>
        /// Id персонажа, добывшего руду.
        /// </summary>
        [JsonPropertyName("character_id")]
        public int CharacterId { get; set; }

        /// <summary>
        /// Дата добычи.
        /// </summary>
        [JsonPropertyName("last_updated")]
        public DateTime LastUpdated { get; set; }

        /// <summary>
        /// Количество добытой руды.
        /// </summary>
        [JsonPropertyName("quantity")]
        public long Quantity { get; set; }

        /// <summary>
        /// Id корпорации, в которой находился персонаж в момент добычи руды.
        /// </summary>
        [JsonPropertyName("recorded_corporation_id")]
        public int RecordedCorporationId { get; set; }

        /// <summary>
        /// Id добытой руды.
        /// </summary>
        [JsonPropertyName("type_id")]
        public int TypeId { get; set; }

        #endregion Properties
    }
}
