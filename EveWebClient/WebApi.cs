using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace EveWebClient
{
    internal static class WebApi
    {
        internal static Dictionary<string, FuzzworkBuySellData> GetFuzzworkPrices(string url)
        {
            var json = Get(url).GetAwaiter().GetResult();
            Dictionary<string, FuzzworkBuySellData> marketData = JsonConvert.DeserializeObject<Dictionary<string, FuzzworkBuySellData>>(json);
            return marketData;
        }

        internal static EsiPrice[] GetEsiPrices(string url)
        {
            var json = Get(url).GetAwaiter().GetResult();
            var data = JsonConvert.DeserializeObject<EsiPrice[]>(json);
            return data;
        }

        internal static async Task<string> Get(string url)
        {
            HttpMessageHandler handler = new HttpClientHandler();
            using (var client = new HttpClient(handler, false))
            {
                using var result = await client.GetAsync(url);
                if (result.IsSuccessStatusCode)
                {
                    return await result.Content.ReadAsStringAsync();
                }
            }
            return "";
        }
    }
}
