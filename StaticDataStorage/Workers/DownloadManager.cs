using EveCommon.Interfaces;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StaticDataStorage.Workers
{
    public class DownloadManager : IDownloadManager
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IFileService _fileService;
        private readonly ILogger<DownloadManager> _logger;

        public DownloadManager(
            IHttpClientFactory httpClientFactory,
            IFileService fileService,
            ILogger<DownloadManager> logger)
        {
            _httpClientFactory = httpClientFactory;
            _fileService = fileService;
            _logger = logger;
        }

        public async Task<(bool Success, string FilePath)> DownloadArchiveAsync(
            string url,
            string destinationPath,
            long estimatedArchiveSize = 100 * 1024 * 1024,
            long estimatedExtractedSize = 1 * 1024 * 1024 * 1024,
            CancellationToken cancellationToken = default)
        {
            try
            {
                // Создаем папку назначения если её нет
                Directory.CreateDirectory(destinationPath);

                var fileName = Path.GetFileName(url);
                if (string.IsNullOrEmpty(fileName))
                    fileName = $"archive_{DateTime.Now:yyyyMMdd_HHmmss}.zip";

                var archivePath = Path.Combine(destinationPath, fileName);
                var tempArchivePath = archivePath + ".tmp";

                // Проверяем наличие свободного места для архива
                if (!await _fileService.CheckDiskSpaceAsync(destinationPath, estimatedArchiveSize))
                {
                    _logger.LogError("Недостаточно места на диске для загрузки архива. Требуется: {EstimatedSize} MB",
                        estimatedArchiveSize / (1024 * 1024));
                    return (false, string.Empty);
                }

                // Проверяем наличие свободного места для распаковки
                if (!await _fileService.CheckDiskSpaceAsync(destinationPath, estimatedArchiveSize + estimatedExtractedSize))
                {
                    _logger.LogError("Недостаточно места на диске для распаковки архива. Требуется: {TotalSize} MB",
                        (estimatedArchiveSize + estimatedExtractedSize) / (1024 * 1024));
                    return (false, string.Empty);
                }

                // Скачиваем архив
                var success = await DownloadFileWithProgressAsync(url, tempArchivePath, cancellationToken);

                if (!success)
                {
                    if (File.Exists(tempArchivePath))
                        File.Delete(tempArchivePath);
                    return (false, string.Empty);
                }

                // Переименовываем временный файл в постоянный
                if (File.Exists(archivePath))
                    File.Delete(archivePath);

                File.Move(tempArchivePath, archivePath);

                // Распаковываем архив
                success = await ExtractArchiveAsync(archivePath, destinationPath, cancellationToken);

                if (!success)
                {
                    // Удаляем архив в случае неудачной распаковки
                    try { File.Delete(archivePath); } catch { }
                    return (false, string.Empty);
                }

                _logger.LogInformation("Архив успешно скачан и распакован: {ArchivePath}", archivePath);
                return (true, archivePath);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при скачивании и распаковке архива");
                return (false, string.Empty);
            }
        }

        private async Task<bool> DownloadFileWithProgressAsync(
            string url,
            string filePath,
            CancellationToken cancellationToken)
        {
            var httpClient = _httpClientFactory.CreateClient();

            // Увеличиваем таймауты для больших файлов
            httpClient.Timeout = TimeSpan.FromMinutes(30);

            using var response = await httpClient.GetAsync(url, HttpCompletionOption.ResponseHeadersRead, cancellationToken);
            response.EnsureSuccessStatusCode();

            var totalBytes = response.Content.Headers.ContentLength ?? -1L;
            var canReportProgress = totalBytes > 0;

            _logger.LogInformation("Начинаем загрузку файла {FileName}. Размер: {Size} MB",
                Path.GetFileName(filePath),
                canReportProgress ? totalBytes / (1024 * 1024) : "неизвестен");

            await using var contentStream = await response.Content.ReadAsStreamAsync(cancellationToken);
            await using var fileStream = new FileStream(filePath, FileMode.Create, FileAccess.Write, FileShare.None, 81920, true);

            var totalReadBytes = 0L;
            var buffer = new byte[81920]; // 80 KB буфер
            var isMoreToRead = true;
            var lastReportTime = DateTime.Now;
            var reportInterval = TimeSpan.FromSeconds(5);

            while (isMoreToRead)
            {
                cancellationToken.ThrowIfCancellationRequested();

                var read = await contentStream.ReadAsync(buffer, 0, buffer.Length, cancellationToken);
                if (read == 0)
                {
                    isMoreToRead = false;
                    continue;
                }

                await fileStream.WriteAsync(buffer, 0, read, cancellationToken);
                totalReadBytes += read;

                // Периодический отчет о прогрессе
                if (canReportProgress && DateTime.Now - lastReportTime > reportInterval)
                {
                    var progressPercentage = (double)totalReadBytes / totalBytes * 100;
                    _logger.LogInformation("Прогресс загрузки: {Progress:F1}% ({CurrentMB}/{TotalMB} MB)",
                        progressPercentage,
                        totalReadBytes / (1024 * 1024),
                        totalBytes / (1024 * 1024));
                    lastReportTime = DateTime.Now;
                }
            }

            _logger.LogInformation("Загрузка завершена: {FileName}", Path.GetFileName(filePath));
            return true;
        }

        private async Task<bool> ExtractArchiveAsync(
            string archivePath,
            string destinationPath,
            CancellationToken cancellationToken)
        {
            try
            {
                _logger.LogInformation("Начинаем распаковку архива: {ArchivePath}", archivePath);

                await using var archiveStream = new FileStream(
                    archivePath,
                    FileMode.Open,
                    FileAccess.Read,
                    FileShare.Read,
                    81920,
                    true);

                using var archive = new ZipArchive(archiveStream, ZipArchiveMode.Read);

                var totalEntries = archive.Entries.Count;
                var processedEntries = 0;
                var lastReportTime = DateTime.Now;
                var reportInterval = TimeSpan.FromSeconds(3);

                foreach (var entry in archive.Entries)
                {
                    cancellationToken.ThrowIfCancellationRequested();

                    var entryDestinationPath = Path.Combine(destinationPath, entry.FullName);

                    // Создаем директорию если требуется
                    if (entry.FullName.EndsWith("/") || string.IsNullOrEmpty(Path.GetExtension(entry.FullName)))
                    {
                        Directory.CreateDirectory(entryDestinationPath);
                        continue;
                    }

                    var entryDirectory = Path.GetDirectoryName(entryDestinationPath);
                    if (!string.IsNullOrEmpty(entryDirectory))
                        Directory.CreateDirectory(entryDirectory);

                    await using var entryStream = entry.Open();
                    await using var fileStream = new FileStream(
                        entryDestinationPath,
                        FileMode.Create,
                        FileAccess.Write,
                        FileShare.None,
                        81920,
                        true);

                    await entryStream.CopyToAsync(fileStream, cancellationToken);
                    processedEntries++;

                    // Периодический отчет о прогрессе
                    if (DateTime.Now - lastReportTime > reportInterval)
                    {
                        var progressPercentage = (double)processedEntries / totalEntries * 100;
                        _logger.LogInformation("Прогресс распаковки: {Progress:F1}% ({Processed}/{Total} файлов)",
                            progressPercentage, processedEntries, totalEntries);
                        lastReportTime = DateTime.Now;
                    }
                }

                _logger.LogInformation("Распаковка завершена успешно. Обработано файлов: {Count}", processedEntries);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при распаковке архива: {ArchivePath}", archivePath);

                // Очищаем распакованные файлы в случае ошибки
                try
                {
                    CleanupExtractedFiles(destinationPath, archivePath);
                }
                catch (Exception cleanupEx)
                {
                    _logger.LogError(cleanupEx, "Ошибка при очистке распакованных файлов");
                }

                return false;
            }
        }

        private void CleanupExtractedFiles(string destinationPath, string archivePath)
        {
            var archiveName = Path.GetFileNameWithoutExtension(archivePath);
            var extractedFolder = Path.Combine(destinationPath, archiveName);

            if (Directory.Exists(extractedFolder))
            {
                Directory.Delete(extractedFolder, true);
                _logger.LogInformation("Очищена папка с распакованными файлами: {Folder}", extractedFolder);
            }
        }
    }
}
