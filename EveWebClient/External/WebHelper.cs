using EveCommon.Interfaces;
using EveCommon.Models;
using EveWebClient.Esi.Models;
using EveWebClient.External.Models;
using EveWebClient.External.Models.Seat;
using EveWebClient.SSO;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using NLog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using System.Web;
using static System.Net.Mime.MediaTypeNames;

namespace EveWebClient.External
{
    /// <summary>
    /// Класс обращения к различным сервисам помимо EVE SSO и EVE Esi.
    /// </summary>
    public class WebHelper
    {
        /// <summary>
        /// Строка обращения к списку аккаунтов сеата.
        /// </summary>
        private const string SeatUsersUrl = "/api/v2/users";
        /// <summary>
        /// Строка обращения к корпоративному валлету сеата.
        /// </summary>
        private const string SeatCorporationWalletJournalUrl = "/api/v2/corporation/wallet-journal";

        private IConfig Config { get; }
        private HttpClient HttpClient { get; }
        protected ILogger<WebHelper> Logger { get; }

        public WebHelper(HttpClient httpClient, IConfig config, ILogger<WebHelper> logger)
        {
            Config = config;
            HttpClient = httpClient;
            Logger = logger;
        }

        #region Получение всех цен

        /// <summary>
        /// Получает актуальные цены на предметы из EVE Esi и Fuzzwork.
        /// </summary>
        /// <param name="ids">Id предметов.</param>
        /// <returns>Список цен.</returns>
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

        /// <summary>
        /// Получает словарь цен набора предметов с сайта Fuzzwork.
        /// </summary>
        /// <param name="uri">Строка запроса для одного ли нескольких предметов.</param>
        /// <returns>Словарь с id предмета и его ценой.</returns>
        public async Task<Dictionary<string, FuzzworkBuySellData>> GetFuzzworkPrices(Uri uri)
        {
            var request = new HttpRequestMessage
            {
                RequestUri = uri,
                Method = HttpMethod.Get,
            };
            string json = "";

            var response = await HttpClient.SendAsync(request).ConfigureAwait(false);
            if (response.IsSuccessStatusCode)
            {
                json = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
            }

            Dictionary<string, FuzzworkBuySellData> marketData = JsonConvert.DeserializeObject<Dictionary<string, FuzzworkBuySellData>>(json);
            return marketData;
        }

        #endregion


        #region Seat

        /// <summary>
        /// Выполняет поиск номера страницы, с которой нужно начать считывание журнала транзакции корпорации из сеата.
        /// </summary>
        /// <param name="corporationId">Id корпорации.</param>
        /// <param name="dateTimeStart">Дата транзакции, для которой ищем номер страницы.</param>
        /// <returns>Номер страницы сеата.</returns>
        public async Task<int> SearchPageSeatCorporationWalletJournal(int corporationId, DateTime dateTimeStart)
        {
            /// Так как сеат предоставляет по REST API данные постранично (по 15 записей на страницу), а число записей и страниц может составлять десятки тысяч.
            /// А фильтр по дате API не поддерживает (в формате odata).
            /// То в целях того, чтобы не тянуть все десятки тысяч данных за прошедшие года, нужно выбрать такую страницу, на которой впервые встречается транзакция за указанную дату.

            var sub1day = dateTimeStart.AddDays(-1);
            var fullUrl = $"{SeatCorporationWalletJournalUrl}/{corporationId}";

            int page = 1, lowPage = 0;
            /// Получаем первую страницу транзакций корпорации.
            /// Если транзакций нет, возвращает -1.
            var currentPage = await CreateSeatTask<CorporationWalletJournal>(page, fullUrl);
            if (currentPage == null || currentPage.WalletTransactions == null)
                return -1;
            /// В полученном ответе есть данные о последней странице.
            int maxPage = currentPage.Meta.last_page;

            /// Если последняя страница = первой, то найден ответ.
            if (page == maxPage)
                return page;

            /// Запрашиваем данные с последней страницы.
            /// Если на последней странице все транзакции по дате меньше нужной даты, значит корпорация не активна (на нужную дату) и ответ -1.
            var pageLast = await CreateSeatTask<CorporationWalletJournal>(maxPage, fullUrl);
            if (pageLast.WalletTransactions.Max(x => x.date) < dateTimeStart)
                return -1;

            /// Указываем, что максимальное число итераций поиска 100, чтобы не попадать в бесконечный цикл в наихудшем случае.
            var iter = 100;

            do
            {
                /// Если текущая страница - первая, и самая старая транзакция на ней больше даты, значит ответ - первая страница.
                var min = currentPage.WalletTransactions.Min(x => x.date);
                if (page == 1 && min > dateTimeStart)
                    return page;

                /// Если на текущей странице есть транзакция, которая попадает в рамки нужной даты, нашли ответ.
                if (currentPage.WalletTransactions.Any(x => sub1day < x.date) && currentPage.WalletTransactions.Any(x => x.date < dateTimeStart))
                    return page;

                if (sub1day < min && min < dateTimeStart)
                    return page;
                else
                {
                    /// Если самая старая транзакция на странице меньше нужной даты
                    if (sub1day > min)
                    {
                        /// Идём по диапазону страниц "вперёд"
                        /// В качестве нижней границы берём текущую
                        /// В качестве верхней границы берём середину промежутка между текущей и максимальной страницей.
                        lowPage = page;
                        page = (page + maxPage) / 2;
                    }
                    /// Если самая старая транзакция на странице больше нужной даты
                    if (min > dateTimeStart)
                    {
                        /// Идём по диапазону страниц "назад"
                        /// В качестве верхней границы берём текущую
                        /// В качестве нижней границы берём середину промежутка между текущей и наименьшей страницей.
                        maxPage = page;
                        page = (page + lowPage) / 2;
                    }
                }
                /// Запрашиваем данные страницы с выбранным номером.
                currentPage = await CreateSeatTask<CorporationWalletJournal>(page, fullUrl);
                iter--;

                /// Если исчерпан лимит попыток, то берём номер последней страницы - не повезло.
                if (iter == 0)
                    return maxPage;
            }
            while (true);
        }

        /// <summary>
        /// Получает данные о транзакциях корпорации из сеата.
        /// </summary>
        /// <param name="corporationId">Id корпорации.</param>
        /// <param name="page">Начальный номер страницы сеата.</param>
        /// <param name="filter">Фильтр в формате OData</param>
        /// <returns>Пара значений: номер последней страницы, с которой были получены данные, и список транзакций.</returns>
        public async Task<KeyValuePair<int, List<WalletTransaction>>> GetSeatCorporationWalletJournal(int corporationId, int? page = null, string filter = null)
        {
            var fullUrl = $"{SeatCorporationWalletJournalUrl}/{corporationId}";

            var list = new List<WalletTransaction>();
            if (!page.HasValue)
                page = 1;
            int startPage = page.Value;
            int lastPage = 1;

            var result = new KeyValuePair<int, List<WalletTransaction>>(lastPage, list);

            try
            {
                /// Получая данные о первой странице, получаем данные еще и о номере последней страницы.
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
            }
            catch (Exception ex)
            {
                //  сохраним последнюю страницу, как предыдущую, с которой не было ошибки
                lastPage = (new [] { page.Value - 1, startPage}).Max();
            }

            result = new KeyValuePair<int, List<WalletTransaction>>(lastPage, list);
            return result;
        }

        /// <summary>
        /// Получает данные обо всех аккаунтах, добавленных в сеат.
        /// </summary>
        /// <returns>Список аккаунтов.</returns>
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

        /// <summary>
        /// Создаёт запрос к сеату.
        /// </summary>
        /// <typeparam name="T">Модуль возвращаемых данных.</typeparam>
        /// <param name="page">Номер страницы сеата.</param>
        /// <param name="url">Полный адрес запроса без указания номера страницы.</param>
        /// <param name="filter">Фильтр в формате OData</param>
        /// <returns></returns>
        private async Task<T> CreateSeatTask<T>(int page, string url, string filter = null) where T : new()
        {
            T result = default;
            string json = "";
            try
            {
                var request = CreateRequest(page, url, filter);

                var response = await HttpClient.SendAsync(request).ConfigureAwait(false);
                if (response.IsSuccessStatusCode)
                {
                    json = await response.Content.ReadAsStringAsync().ConfigureAwait(false);

                    result = JsonConvert.DeserializeObject<T>(json);
                }
                else
                {
                    Logger.LogError("Error during CreateSeatTask()\r\nStatus code:" + response.StatusCode.ToString() + " " + response.ReasonPhrase);
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
