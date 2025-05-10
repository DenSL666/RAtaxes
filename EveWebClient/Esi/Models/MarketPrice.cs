using EveCommon;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace EveWebClient.Esi.Models
{
    public class MarketPrice : ModelBase<MarketPrice>
    {
        static MarketPrice()
        {
            Path = DIManager.Configuration.GetValue<string>("Runtime:PathEsiPrices");
            Path = System.IO.Path.Combine(AppContext.BaseDirectory, Path);
        }

        [System.Text.Json.Serialization.JsonIgnore]
        static string Path { get; }

        #region Properties

        [JsonPropertyName("adjusted_price")]
        public double? AdjustedPrice { get; set; }

        [JsonPropertyName("average_price")]
        public double? AveragePrice { get; set; }

        [JsonPropertyName("type_id")]
        public int TypeId { get; set; }

        #endregion Properties

        public static MarketPrice[] Read()
        {
            MarketPrice[] marketPrices;
            using (var reader = new StreamReader(Path))
            {
                marketPrices = JsonConvert.DeserializeObject<MarketPrice[]>(reader.ReadToEnd());
            }
            return marketPrices;
        }

        public static void Save(IEnumerable<MarketPrice> marketPrices)
        {
            using (var writer = new StreamWriter(Path))
            {
                writer.Write(JsonConvert.SerializeObject(marketPrices));
            }
        }
    }
}
