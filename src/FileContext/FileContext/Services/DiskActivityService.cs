using System;
using System.Collections.Generic;
using System.Management;

namespace FileContext.Services;

public static class DiskActivityService
{
    public static Dictionary<string, (ulong Read, ulong Write)> GetLogicalDiskActivity()
    {
        Dictionary<string, (ulong Read, ulong Write)> result = new();

        try
        {
            using ManagementObjectSearcher searcher = new(
                "SELECT Name, DiskReadBytesPerSec, DiskWriteBytesPerSec " +
                "FROM Win32_PerfFormattedData_PerfDisk_LogicalDisk");

            foreach (ManagementObject item in searcher.Get())
            {
                string? name = item["Name"]?.ToString();

                if (string.IsNullOrWhiteSpace(name) ||
                    name == "_Total")
                {
                    continue;
                }

                ulong read = Convert.ToUInt64(
                    item["DiskReadBytesPerSec"] ?? 0UL);

                ulong write = Convert.ToUInt64(
                    item["DiskWriteBytesPerSec"] ?? 0UL);

                result[name] = (read, write);
            }
        }
        catch
        {
            // İlk sürümde performans verisi okunamazsa
            // Storage Overview çalışmaya devam etsin.
        }

        return result;
    }
}