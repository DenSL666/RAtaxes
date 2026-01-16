using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EveCommon.Interfaces
{
    public interface IDownloadManager
    {
        Task<(bool Success, string FilePath)> DownloadArchiveAsync(
            string url,
            string destinationPath,
            long estimatedArchiveSize = 100 * 1024 * 1024, // 100 MB по умолчанию
            long estimatedExtractedSize = 1 * 1024 * 1024 * 1024, // 1 GB по умолчанию
            CancellationToken cancellationToken = default);
    }
}
