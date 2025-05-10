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
        [JsonPropertyName("last_updated")]
        public DateTime LastUpdated { get; set; }

        [JsonPropertyName("observer_id")]
        public long ObserverId { get; set; }

        [JsonPropertyName("observer_type")]
        public string ObserverType { get; set; }
    }
}
