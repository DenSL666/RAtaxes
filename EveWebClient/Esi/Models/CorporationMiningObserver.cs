using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace EveWebClient.Esi.Models
{
    public class CorporationMiningObserver : ModelBase<CorporationMiningObserver>
    {
        /// <summary>
        /// Дата добычи руды.
        /// </summary>
        [JsonPropertyName("last_updated")]
        public DateTime LastUpdated { get; set; }

        /// <summary>
        /// Id структуры, где была добыта руда.
        /// </summary>
        [JsonPropertyName("observer_id")]
        public long ObserverId { get; set; }

        /// <summary>
        /// Тип структуры?
        /// </summary>
        [JsonPropertyName("observer_type")]
        public string ObserverType { get; set; }
    }
}
