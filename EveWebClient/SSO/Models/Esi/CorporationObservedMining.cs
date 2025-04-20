using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace EveWebClient.SSO.Models.Esi
{
    public class CorporationObservedMining : ModelBase<CorporationObservedMining>
    {
        #region Properties

        [JsonPropertyName("character_id")]
        public int CharacterId { get; set; }

        [JsonPropertyName("last_updated")]
        public DateTime LastUpdated { get; set; }

        [JsonPropertyName("quantity")]
        public long Quantity { get; set; }

        [JsonPropertyName("recorded_corporation_id")]
        public int RecordedCorporationId { get; set; }

        [JsonPropertyName("type_id")]
        public int TypeId { get; set; }

        #endregion Properties
    }
}
