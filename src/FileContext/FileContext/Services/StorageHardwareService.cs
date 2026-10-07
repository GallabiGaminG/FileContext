using System;
using System.Collections.Generic;
using System.Linq;
using System.Management;

namespace FileContext.Services;

public static class StorageHardwareService
{
    public static Dictionary<string, (string Model, string MediaType, string BusType)>
        GetLogicalDriveHardwareInfo()
    {
        Dictionary<string, (string Model, string MediaType, string BusType)> result =
            new(StringComparer.OrdinalIgnoreCase);

        try
        {
            List<(string FriendlyName, ushort MediaType, ushort BusType)> physicalDisks =
                GetPhysicalDiskInfo();

            using ManagementObjectSearcher diskSearcher = new(
                "SELECT DeviceID, Model FROM Win32_DiskDrive");

            foreach (ManagementObject disk in diskSearcher.Get())
            {
                string model =
                    disk["Model"]?.ToString()?.Trim() ?? "";

                var modernDisk = physicalDisks.FirstOrDefault(
                    physical =>
                        IsSameDiskName(model, physical.FriendlyName));

                string mediaType =
                    GetMediaTypeText(modernDisk.MediaType);

                string busType =
                    GetBusTypeText(modernDisk.BusType);

                foreach (ManagementObject partition in
                         disk.GetRelated("Win32_DiskPartition"))
                {
                    foreach (ManagementObject logicalDisk in
                             partition.GetRelated("Win32_LogicalDisk"))
                    {
                        string driveLetter =
                            logicalDisk["DeviceID"]?.ToString() ?? "";

                        if (string.IsNullOrWhiteSpace(driveLetter))
                            continue;

                        result[driveLetter] =
                            (
                                model,
                                mediaType,
                                busType
                            );
                    }
                }
            }
        }
        catch
        {
            // Donanım bilgisi okunamazsa Storage Overview çalışmaya devam etsin.
        }

        return result;
    }

    public static List<(string FriendlyName, ushort MediaType, ushort BusType)>
        GetPhysicalDiskInfo()
    {
        List<(string FriendlyName, ushort MediaType, ushort BusType)> result = new();

        try
        {
            ManagementScope scope = new(
                @"\\.\ROOT\Microsoft\Windows\Storage");

            scope.Connect();

            ObjectQuery query = new(
                "SELECT FriendlyName, MediaType, BusType FROM MSFT_PhysicalDisk");

            using ManagementObjectSearcher searcher =
                new(scope, query);

            foreach (ManagementObject disk in searcher.Get())
            {
                string friendlyName =
                    disk["FriendlyName"]?.ToString()?.Trim() ?? "";

                ushort mediaType =
                    disk["MediaType"] is ushort media
                        ? media
                        : (ushort)0;

                ushort busType =
                    disk["BusType"] is ushort bus
                        ? bus
                        : (ushort)0;

                result.Add(
                    (
                        friendlyName,
                        mediaType,
                        busType
                    ));
            }
        }
        catch
        {
            // Modern storage bilgisi alınamazsa boş liste dön.
        }

        return result;
    }

    private static bool IsSameDiskName(
        string first,
        string second)
    {
        if (string.IsNullOrWhiteSpace(first) ||
            string.IsNullOrWhiteSpace(second))
        {
            return false;
        }

        string normalizedFirst =
            NormalizeDiskName(first);

        string normalizedSecond =
            NormalizeDiskName(second);

        return
            normalizedFirst.Equals(
                normalizedSecond,
                StringComparison.OrdinalIgnoreCase)
            ||
            normalizedFirst.Contains(
                normalizedSecond,
                StringComparison.OrdinalIgnoreCase)
            ||
            normalizedSecond.Contains(
                normalizedFirst,
                StringComparison.OrdinalIgnoreCase);
    }

    private static string NormalizeDiskName(string value)
    {
        return string.Join(
            " ",
            value.Split(
                ' ',
                StringSplitOptions.RemoveEmptyEntries));
    }

    private static string GetMediaTypeText(ushort mediaType)
    {
        return mediaType switch
        {
            3 => "HDD",
            4 => "SSD",
            5 => "SCM",
            _ => "Unknown"
        };
    }

    private static string GetBusTypeText(ushort busType)
    {
        return busType switch
        {
            1 => "SCSI",
            2 => "ATAPI",
            3 => "ATA",
            4 => "IEEE 1394",
            6 => "Fibre Channel",
            7 => "USB",
            8 => "RAID",
            9 => "iSCSI",
            10 => "SAS",
            11 => "SATA",
            12 => "SD",
            13 => "MMC",
            14 => "Virtual",
            15 => "File Backed Virtual",
            16 => "Storage Spaces",
            17 => "NVMe",
            18 => "SCM",
            19 => "UFS",
            _ => "Unknown"
        };
    }
}