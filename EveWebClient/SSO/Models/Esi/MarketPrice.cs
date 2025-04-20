using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
using Newtonsoft.Json.Serialization;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace EveWebClient.SSO.Models.Esi
{
    public class MarketPrice : ModelBase<MarketPrice>
    {
        #region Properties

        [JsonPropertyName("adjusted_price")]
        public double? AdjustedPrice { get; set; }

        [JsonPropertyName("average_price")]
        public double? AveragePrice { get; set; }

        [JsonPropertyName("type_id")]
        public int TypeId { get; set; }

        #endregion Properties

        public static MarketPrice[] Read(string path)
        {
            MarketPrice[] marketPrices;
            using (var reader = new StreamReader(path))
            {
                marketPrices = JsonConvert.DeserializeObject<MarketPrice[]>(reader.ReadToEnd());
            }
            return marketPrices;
        }

        public static void Save(IEnumerable<MarketPrice> marketPrices, string path)
        {
            using (var writer = new StreamWriter(path))
            {
                writer.Write(JsonConvert.SerializeObject(marketPrices));
            }
        }
    }
}
