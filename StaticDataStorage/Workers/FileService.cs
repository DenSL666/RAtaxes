using EveCommon.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StaticDataStorage.Workers
{
    public class FileService : IFileService
    {
        public async Task<bool> CheckDiskSpaceAsync(string path, long requiredBytes)
        {
            return await Task.Run(() => GetAvailableDiskSpace(path) >= requiredBytes);
        }

        public long GetAvailableDiskSpace(string path)
        {
            var driveInfo = new DriveInfo(Path.GetPathRoot(Path.GetFullPath(path))!);
            return driveInfo.AvailableFreeSpace;
        }
    }
}
