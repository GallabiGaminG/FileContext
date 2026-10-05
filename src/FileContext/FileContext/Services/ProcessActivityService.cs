using System;
using System.Collections.Generic;
using System.Linq;
using System.Management;
using FileContext.Models;

namespace FileContext.Services;

public static class ProcessActivityService
{
    public static List<ProcessIoInfo> GetActiveProcesses()
    {
        List<ProcessIoInfo> processes = new();

        try
        {
            using ManagementObjectSearcher searcher = new(
                "SELECT Name, IDProcess, IOReadBytesPerSec, IOWriteBytesPerSec " +
                "FROM Win32_PerfFormattedData_PerfProc_Process");

            foreach (ManagementObject item in searcher.Get())
            {
                uint processId =
                    Convert.ToUInt32(item["IDProcess"] ?? 0U);

                if (processId == 0)
                    continue;

                ulong read =
                    Convert.ToUInt64(item["IOReadBytesPerSec"] ?? 0UL);

                ulong write =
                    Convert.ToUInt64(item["IOWriteBytesPerSec"] ?? 0UL);

                if (read == 0 && write == 0)
                    continue;

                processes.Add(new ProcessIoInfo
                {
                    Name = item["Name"]?.ToString() ?? "Unknown",
                    ProcessId = processId,

                    ReadBytesPerSecond = read,
                    WriteBytesPerSecond = write,

                    ReadSpeedText = FormatSpeed(read),
                    WriteSpeedText = FormatSpeed(write)
                });
            }
        }
        catch
        {
            // Process performance data unavailable:
            // Storage Overview should continue working.
        }

        return processes
            .OrderByDescending(x => x.TotalBytesPerSecond)
            .Take(10)
            .ToList();
    }

    private static string FormatSpeed(ulong bytesPerSecond)
    {
        const double MB = 1024 * 1024;

        return $"{bytesPerSecond / MB:0.0} MB/s";
    }
}