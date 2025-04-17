using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EveWebClient.EsiModels
{
    internal class EsiPrice
    {
        [JsonProperty("adjusted_price")]
        public double? AdjustedPrice { get; set; }

        [JsonProperty("average_price")]
        public double? AveragePrice { get; set; }

        [JsonProperty("type_id")]
        public long? TypeId { get; set; }
    }
}
