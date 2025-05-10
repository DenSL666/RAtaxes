using EveCommon.Interfaces;
using EveCommon.Models;
using EveWebClient.Esi.Models;
using EveWebClient.External.Models;
using EveWebClient.External.Models.Seat;
using EveWebClient.SSO;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using System.Web;
using static System.Net.Mime.MediaTypeNames;

namespace EveWebClient.External
{

    public class WebHelper
    {
        private const string SeatUsersUrl = "/api/v2/users";
        private const string SeatCorporationWalletJournalUrl = "/api/v2/corporation/wallet-journal";

        private IConfig Config { get; }
        private IHttpClient GlobalHttpClient { get; }

        public WebHelper(IHttpClient globalHttpClient, IConfig config)
        {
            Config = config;
            GlobalHttpClient = globalHttpClient;
        }

        #region Получение всех цен

        public async Task<List<Price>> GetPrices(IEnumerable<string> ids)
        {
            var result = new List<Price>();

            if (ids != null && ids.Any())
            {
                var esiData = MarketPrice.Read();

                var idString = string.Join(",", ids);
                var fuzzworkData = await GetFuzzworkPrices(new Uri(Config.FuzzworkPricesUrl + idString));

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

        #endregion

        #region Fuzzwork

        public async Task<Dictionary<string, FuzzworkBuySellData>> GetFuzzworkPrices(Uri uri)
        {
            var request = new HttpRequestMessage
            {
                RequestUri = uri,
                Method = HttpMethod.Get,
            };
            string json = "";

            var response = await GlobalHttpClient.HttpClient.SendAsync(request).ConfigureAwait(false);
            if (response.IsSuccessStatusCode)
            {
                json = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
            }

            Dictionary<string, FuzzworkBuySellData> marketData = JsonConvert.DeserializeObject<Dictionary<string, FuzzworkBuySellData>>(json);
            return marketData;
        }

        #endregion


        #region Seat

        public async Task<int> SearchPageSeatCorporationWalletJournal(int corporationId, DateTime dateTimeStart)
        {
            var sub1day = dateTimeStart.AddDays(-1);
            var fullUrl = $"{SeatCorporationWalletJournalUrl}/{corporationId}";

            int page = 1, lowPage = 0;
            var page1 = await CreateSeatTask<CorporationWalletJournal>(page, fullUrl);
            if (page1 == null || page1.WalletTransactions == null)
                return -1;
            int maxPage = page1.Meta.last_page;

            if (page == maxPage)
                return page;

            var pageLast = await CreateSeatTask<CorporationWalletJournal>(maxPage, fullUrl);
            if (pageLast.WalletTransactions.Max(x => x.date) < dateTimeStart)
                return -1;

            do
            {
                var min = page1.WalletTransactions.Min(x => x.date);
                if (page == 1 && min > dateTimeStart)
                    return page;

                if (page1.WalletTransactions.Any(x => sub1day < x.date) && page1.WalletTransactions.Any(x => x.date < dateTimeStart))
                    return page;

                if (sub1day < min && min < dateTimeStart)
                    return page;
                else
                {
                    if (sub1day > min)
                    {
                        lowPage = page;
                        page = (page + maxPage) / 2;
                    }
                    if (min > dateTimeStart)
                    {
                        maxPage = page;
                        page = (page + lowPage) / 2;
                    }
                }
                page1 = await CreateSeatTask<CorporationWalletJournal>(page, fullUrl);
            }
            while (true);
        }

        public async Task<KeyValuePair<int, List<WalletTransaction>>> GetSeatCorporationWalletJournal(int corporationId, int? page = null, string filter = null)
        {
            var fullUrl = $"{SeatCorporationWalletJournalUrl}/{corporationId}";

            var list = new List<WalletTransaction>();
            if (!page.HasValue)
                page = 1;
            int lastPage = 1;

            var result = new KeyValuePair<int, List<WalletTransaction>>(lastPage, list);

            var page1 = await CreateSeatTask<CorporationWalletJournal>(page.Value, fullUrl, filter);
            page++;
            if (page1 == null || page1.WalletTransactions == null)
                return result;
            list.AddRange(page1.WalletTransactions);

            lastPage = page1.Meta.last_page;
            for (; page <= lastPage; page++)
            {
                var _result = await CreateSeatTask<CorporationWalletJournal>(page.Value, fullUrl);

                list.AddRange(_result.WalletTransactions);
            }

            result = new KeyValuePair<int, List<WalletTransaction>>(lastPage, list);
            return result;
        }

        public async Task<List<UserInfo>> GetSeatUserInfo()
        {
            var result = new List<UserInfo>();
            int page = 1, maxPage = 1;
            var page1 = await CreateSeatTask<UserList>(page, SeatUsersUrl);
            page++;
            if (page1 == null || page1.UserInfoArray == null)
                return result;
            result.AddRange(page1.UserInfoArray);
            if (page1.Meta != null && page1.Meta.last_page > 1)
            {
                var taskArray = Enumerable.Range(page, page1.Meta.last_page - 1).Select(_page => CreateSeatTask<UserList>(_page, SeatUsersUrl)).ToArray();
                var taskResults = await Task.WhenAll(taskArray);

                result.AddRange(taskResults.Where(x => x != null && x.UserInfoArray != null).SelectMany(x => x.UserInfoArray));
            }
            return result;
        }

        private async Task<T> CreateSeatTask<T>(int page, string url, string filter = null) where T : new()
        {
            T result = default;
            string json = "";
            try
            {
                var request = CreateRequest(page, url, filter);

                var response = await GlobalHttpClient.HttpClient.SendAsync(request).ConfigureAwait(false);
                if (response.IsSuccessStatusCode)
                {
                    json = await response.Content.ReadAsStringAsync().ConfigureAwait(false);

                    result = JsonConvert.DeserializeObject<T>(json);
                }
            }
            catch { }
            return result;
        }

        private HttpRequestMessage CreateRequest(int page, string url, string filter = null)
        {
            var queryParams = HttpUtility.ParseQueryString(String.Empty);
            queryParams.Add("page", page.ToString());

            if (!string.IsNullOrEmpty(filter))
            {

                queryParams.Add(Uri.EscapeDataString("$filter"), Uri.EscapeDataString(filter));
            }

            var builder = new UriBuilder(Config.SeatParams.SeatUrl)
            {
                Path = url,
                Query = queryParams.ToString(),
            };
            var request = new HttpRequestMessage
            {
                RequestUri = builder.Uri,
                Method = HttpMethod.Get,
            };
            request.Headers.Add("X-Token", Config.SeatParams.SeatToken);
            return request;
        }

        #endregion

    }
}
