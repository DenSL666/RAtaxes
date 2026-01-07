using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EveCommon.Interfaces
{
    public interface IFileService
    {
        Task<bool> CheckDiskSpaceAsync(string path, long requiredBytes);
        long GetAvailableDiskSpace(string path);
    }
}
