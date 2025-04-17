using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using EveWebClient.External;
using EveWebClient.EsiModels;

namespace EveWebClient.External
{
    public sealed class Price
    {
        internal Price()
        {
            Id = string.Empty;
        }

        public string Id { get; private set; }
        public double JitaBuy { get; private set; }
        public double JitaSell { get; private set; }
        public double JitaSplit { get; private set; }
        public double EveAverage { get; private set; }

        public const string FuzzworkUrl = "https://market.fuzzwork.co.uk/aggregates/?region=30000142&types=";
        public const string EsiUrl = "https://esi.evetech.net/latest/markets/prices/?datasource=tranquility";

        public static List<Price> GetPrices(IEnumerable<string> ids)
        {
            var result = new List<Price>();

            if (ids != null && ids.Any())
            {
                var idString = string.Join(",", ids);
                var fullFuzzworkUrl = FuzzworkUrl + idString;
                var fuzzworkData = WebApi.GetFuzzworkPrices(fullFuzzworkUrl);

                var esiData = WebApi.GetEsiPrices(EsiUrl);

                foreach (var id in ids)
                {
                    var price = new Price
                    {
                        Id = id,
                    };

                    if (fuzzworkData.ContainsKey(id))
                    {
                        var p1 = fuzzworkData[id];
                        if (p1 != null && p1.Sell != null && p1.Sell.Min.HasValue && p1.Sell.Min.Value > 0)
                            price.JitaSell = p1.Sell.Min.Value;
                        if (p1 != null && p1.Buy != null && p1.Buy.Max.HasValue && p1.Buy.Max.Value > 0)
                            price.JitaBuy = p1.Buy.Max.Value;

                        price.JitaSplit = (price.JitaSell + price.JitaBuy) / 2;
                    }
                    if (long.TryParse(id, out long _id))
                    {
                        var found = esiData.FirstOrDefault(x => x.TypeId == _id);
                        if (found != null && found.AveragePrice.HasValue && found.AveragePrice.Value > 0)
                        {
                            price.EveAverage = found.AveragePrice.Value;
                        }
                    }
                    result.Add(price);
                }
            }

            return result;
        }
    }
}
