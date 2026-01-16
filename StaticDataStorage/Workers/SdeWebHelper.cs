using EveCommon.Interfaces;
using EveCommon.Workers;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using StaticDataStorage.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StaticDataStorage.Workers
{
    public class SdeWebHelper : ApiWebHelper
    {
        /// <summary>
        /// Строка обращения к информации о текущей версии SDE.
        /// </summary>
        private const string SdeVersionUrl = "https://developers.eveonline.com/static-data/tranquility/latest.jsonl";

        /// <summary>
        /// Строка загрузки архива SDE.
        /// </summary>
        private const string SdeDownloadUrl = "https://developers.eveonline.com/static-data/eve-online-static-data-latest-yaml.zip";

        /// <summary>
        /// Название директории для сохранения загружаемого файла.
        /// </summary>
        private const string DownloadFolderName = "Downloads";

        private readonly IDownloadManager _downloadManager;

        public SdeWebHelper(HttpClient httpClient, IConfig config, ILogger<SdeWebHelper> logger, IDownloadManager downloadManager) : base(httpClient, config, logger)
        {
            _downloadManager = downloadManager;
        }

        #region Eve static data export (SDE)

        public async Task<SdeVersionData> GetSdeVersion()
        {
            return await GetJsonRequest<SdeVersionData>(SdeVersionUrl);
        }

        public async Task<(bool Success, string FilePath)> DownloadSdeArchiveAsync(string destinationFolder = null, CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Starting SDE archive download...");

            // Скачиваем архив
            var result = await _downloadManager.DownloadArchiveAsync(
                SdeDownloadUrl,
                destinationFolder ?? DownloadFolderName,
                estimatedArchiveSize: 80 * 1024 * 1024,    // ~80 MB
                estimatedExtractedSize: 1 * 1024 * 1024 * 1024, // ~1 GB
                cancellationToken);

            return result;
        }

        #endregion
    }
}
