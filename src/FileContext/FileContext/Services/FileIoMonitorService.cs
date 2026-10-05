using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using FileContext.Models;
using Microsoft.Diagnostics.Tracing.Parsers;
using Microsoft.Diagnostics.Tracing.Session;

namespace FileContext.Services;

public sealed class FileIoMonitorService : IDisposable
{
    private readonly ConcurrentQueue<FileIoActivity> _events = new();

    private TraceEventSession? _session;
    private Task? _processingTask;
    private CancellationTokenSource? _cancellation;

    public bool IsRunning => _session != null;

    public void Start()
    {
        if (IsRunning)
            return;

        _cancellation = new CancellationTokenSource();

        _session = new TraceEventSession(
            "FileContext-FileIoMonitor");

        _session.StopOnDispose = true;

        _session.EnableKernelProvider(
            KernelTraceEventParser.Keywords.FileIO |
            KernelTraceEventParser.Keywords.FileIOInit);

        _session.Source.Kernel.FileIORead += data =>
        {
            AddEvent(
                data.ProcessID,
                data.FileName,
                "READ",
                data.IoSize);
        };

        _session.Source.Kernel.FileIOWrite += data =>
        {
            AddEvent(
                data.ProcessID,
                data.FileName,
                "WRITE",
                data.IoSize);
        };

        _processingTask = Task.Run(() =>
        {
            try
            {
                _session.Source.Process();
            }
            catch
            {
                // Session durdurulurken Process() çıkabilir.
            }
        });
    }

    private void AddEvent(
        int processId,
        string? path,
        string operation,
        int bytes)
    {
        if (string.IsNullOrWhiteSpace(path))
            return;

        string processName = "Unknown";

        try
        {
            Process process =
                Process.GetProcessById(processId);

            processName = process.ProcessName;
        }
        catch
        {
            // Process event sonrası kapanmış olabilir.
        }

        _events.Enqueue(new FileIoActivity
        {
            ProcessId = processId,
            ProcessName = processName,
            Path = path,
            Operation = operation,
            Bytes = bytes,
            Timestamp = DateTime.Now
        });

        while (_events.Count > 5000)
        {
            _events.TryDequeue(out _);
        }
    }

    public List<FileIoActivity> GetRecentEvents(int maxCount = 100)
    {
        return _events
            .ToArray()
            .TakeLast(maxCount)
            .Reverse()
            .ToList();
    }

    public List<FileIoActivity> GetRecentEventsSnapshot(
    int maxCount = 500)
    {
        return _events
            .ToArray()
            .TakeLast(maxCount)
            .ToList();
    }

    public void Dispose()
    {
        try
        {
            _session?.Dispose();
        }
        catch
        {
        }

        _session = null;

        _cancellation?.Cancel();
        _cancellation?.Dispose();
        _cancellation = null;
    }
}