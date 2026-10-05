using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileContext.Models;

public class FileIoActivity
{
    public int ProcessId { get; set; }

    public string ProcessName { get; set; } = "";

    public string Path { get; set; } = "";

    public string Operation { get; set; } = "";

    public long Bytes { get; set; }

    public DateTime Timestamp { get; set; }
}