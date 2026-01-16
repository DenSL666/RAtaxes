using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

using EveCommon.Interfaces;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EveCommon.Workers
{
    /// <summary>
    /// Базовый класс для API-запросов
    /// </summary>
    public abstract class ApiWebHelper
    {
        protected readonly HttpClient _httpClient;
        protected readonly IConfig _config;
        protected readonly ILogger _logger;

        protected ApiWebHelper(HttpClient httpClient, IConfig config, ILogger logger)
        {
            _httpClient = httpClient;
            _config = config;
            _logger = logger;
        }

        protected async Task<T> GetJsonRequest<T>(string url) where T : class, new()
        {
            try
            {
                var response = await _httpClient.GetAsync(url);

                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();
                    return JsonConvert.DeserializeObject<T>(json);
                }

                _logger.LogError("HTTP error {StatusCode} for URL: {Url}", response.StatusCode, url);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during GET request to {Url}", url);
            }

            return new T();
        }
    }
}
