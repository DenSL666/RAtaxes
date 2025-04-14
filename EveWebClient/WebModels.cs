using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EveWebClient
{
    internal class FuzzworkBuySellData
    {
        [JsonProperty("buy")]
        public FuzzworkPrice Buy { get; set; }

        [JsonProperty("sell")]
        public FuzzworkPrice Sell { get; set; }
    }

    internal class FuzzworkPrice
    {
        [JsonProperty("weightedAverage")]
        public double? WeightedAverage { get; set; }

        [JsonProperty("max")]
        public double? Max { get; set; }

        [JsonProperty("min")]
        public double? Min { get; set; }

        [JsonProperty("stddev")]
        public double? Stddev { get; set; }

        [JsonProperty("median")]
        public double? Median { get; set; }

        [JsonProperty("volume")]
        public double? Volume { get; set; }

        [JsonProperty("orderCount")]
        public int? OrderCount { get; set; }

        [JsonProperty("percentile")]
        public double? Percentile { get; set; }
    }

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
