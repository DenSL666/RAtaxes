using EveWebClient.External.Models;
using EveWebClient.External.Models.Seat;
using EveWebClient.SSO;
using EveWebClient.SSO.Models;
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

    public class WebHelper : APIBase
    {
        private const string SeatUsersUrl = "/api/v2/users";
        private const string SeatCorporationWalletJournalUrl = "/api/v2/corporation/wallet-journal";

        public WebHelper(OAuthHelper oauthHelper, Config config) : base(oauthHelper)
        {
            Config = config;
        }

        private Config Config { get; set; }

        public async Task<Dictionary<string, FuzzworkBuySellData>> GetFuzzworkPrices(Uri uri)
        {
            var request = new HttpRequestMessage
            {
                RequestUri = uri,
                Method = HttpMethod.Get,
            };
            string json = "";

            var response = await HTTP.SendAsync(request).ConfigureAwait(false);
            if (response.IsSuccessStatusCode)
            {
                json = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
            }

            Dictionary<string, FuzzworkBuySellData> marketData = JsonConvert.DeserializeObject<Dictionary<string, FuzzworkBuySellData>>(json);
            return marketData;
        }

        public async Task<int> SearchPageSeatCorporationWalletJournal(int corporationId, DateTime dateTimeStart)
        {
            var sub1day = dateTimeStart.AddDays(-1);
            var fullUrl = $"{SeatCorporationWalletJournalUrl}/{corporationId}";

            int page = 1, lowPage = 0;
            var page1 = await CreateUserInfoTask2<CorporationWalletJournal>(page, fullUrl);
            if (page1 == null || page1.WalletTransactions == null)
                return -1;
            int maxPage = page1.Meta.last_page;

            if (page == maxPage)
                return page;

            var pageLast = await CreateUserInfoTask2<CorporationWalletJournal>(maxPage, fullUrl);
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
                page1 = await CreateUserInfoTask2<CorporationWalletJournal>(page, fullUrl);
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

            var page1 = await CreateUserInfoTask2<CorporationWalletJournal>(page.Value, fullUrl, filter);
            page++;
            if (page1 == null || page1.WalletTransactions == null)
                return result;
            list.AddRange(page1.WalletTransactions);

            lastPage = page1.Meta.last_page;
            for (; page <= lastPage; page++)
            {
                var _result = await CreateUserInfoTask2<CorporationWalletJournal>(page.Value, fullUrl);

                list.AddRange(_result.WalletTransactions);
            }

            result = new KeyValuePair<int, List<WalletTransaction>>(lastPage, list);
            return result;
        }

        public async Task<List<UserInfo>> GetSeatUserInfo()
        {
            var result = new List<UserInfo>();
            int page = 1, maxPage = 1;
            var page1 = await CreateUserInfoTask2<UserList>(page, SeatUsersUrl);
            page++;
            if (page1 == null || page1.UserInfoArray == null)
                return result;
            result.AddRange(page1.UserInfoArray);
            if (page1.Meta != null && page1.Meta.last_page > 1)
            {
                var taskArray = Enumerable.Range(page, page1.Meta.last_page - 1).Select(_page => CreateUserInfoTask2<UserList>(_page, SeatUsersUrl)).ToArray();
                var taskResults = await Task.WhenAll(taskArray);

                result.AddRange(taskResults.Where(x => x != null && x.UserInfoArray != null).SelectMany(x => x.UserInfoArray));
            }
            return result;
        }

        private async Task<T> CreateUserInfoTask2<T>(int page, string url, string filter = null) where T : new()
        {
            T result = default;
            string json = "";
            try
            {
                var request = CreateRequest(page, url, filter);

                var response = await HTTP.SendAsync(request).ConfigureAwait(false);
                if (response.IsSuccessStatusCode)
                {
                    json = await response.Content.ReadAsStringAsync().ConfigureAwait(false);

                    result = JsonConvert.DeserializeObject<T>(json);
                }
            }
            catch { }
            return result;
        }

        private async Task<UserList> CreateUserInfoTask(int page, string url)
        {
            UserList result = null;
            string json = "";
            try
            {
                var request = CreateRequest(page, url);

                var response = await HTTP.SendAsync(request).ConfigureAwait(false);
                if (response.IsSuccessStatusCode)
                {
                    json = await response.Content.ReadAsStringAsync().ConfigureAwait(false);

                    result = JsonConvert.DeserializeObject<UserList>(json);
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
    }
}
