using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileContext;

public class DriveInfoModel
{
    public string Name { get; set; } = "";
    public string VolumeLabel { get; set; } = "";
    public string DriveType { get; set; } = "";

    public long TotalSize { get; set; }
    public long UsedSize { get; set; }
    public long FreeSpace { get; set; }

    public double UsedPercentage { get; set; }

    public string TotalSizeText { get; set; } = "";
    public string UsedSizeText { get; set; } = "";
    public string FreeSpaceText { get; set; } = "";
}