using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileContext.Models;

public class ProcessIoInfo
{
    public string Name { get; set; } = "";
    public uint ProcessId { get; set; }

    public ulong ReadBytesPerSecond { get; set; }
    public ulong WriteBytesPerSecond { get; set; }

    public string ReadSpeedText { get; set; } = "";
    public string WriteSpeedText { get; set; } = "";

    public ulong TotalBytesPerSecond =>
        ReadBytesPerSecond + WriteBytesPerSecond;
}