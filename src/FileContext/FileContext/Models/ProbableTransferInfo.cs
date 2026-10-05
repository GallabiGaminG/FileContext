using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileContext.Models;

public class ProbableTransferInfo
{
    public int ProcessId { get; set; }

    public string ProcessName { get; set; } = "";

    public string SourcePath { get; set; } = "";

    public string TargetPath { get; set; } = "";

    public string Confidence { get; set; } = "";

    public DateTime Timestamp { get; set; }
}