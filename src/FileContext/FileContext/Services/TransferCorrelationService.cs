using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using FileContext.Models;

namespace FileContext.Services;

public static class TransferCorrelationService
{
    public static List<ProbableTransferInfo> FindProbableTransfers(
        IEnumerable<FileIoActivity> events)
    {
        List<FileIoActivity> recentEvents = events
            .Where(x =>
                !string.IsNullOrWhiteSpace(x.Path) &&
                (x.Operation == "READ" || x.Operation == "WRITE"))
            .OrderByDescending(x => x.Timestamp)
            .ToList();

        List<ProbableTransferInfo> results = new();

        List<FileIoActivity> reads = recentEvents
            .Where(x => x.Operation == "READ")
            .ToList();

        List<FileIoActivity> writes = recentEvents
            .Where(x => x.Operation == "WRITE")
            .ToList();

        foreach (FileIoActivity read in reads)
        {
            string readDrive = GetDrive(read.Path);
            string readFileName = GetFileNameSafe(read.Path);

            if (string.IsNullOrWhiteSpace(readDrive) ||
                string.IsNullOrWhiteSpace(readFileName))
            {
                continue;
            }

            ProbableTransferCandidate? bestCandidate = null;

            foreach (FileIoActivity write in writes)
            {
                string writeDrive = GetDrive(write.Path);

                if (string.IsNullOrWhiteSpace(writeDrive))
                    continue;

                if (string.Equals(
                    readDrive,
                    writeDrive,
                    StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                double timeDifference =
                    Math.Abs(
                        (write.Timestamp - read.Timestamp)
                        .TotalSeconds);

                if (timeDifference > 5)
                    continue;

                int score = 0;

                string writeFileName =
                    GetFileNameSafe(write.Path);

                if (string.Equals(
                    readFileName,
                    writeFileName,
                    StringComparison.OrdinalIgnoreCase))
                {
                    score += 50;
                }

                if (read.ProcessId == write.ProcessId)
                {
                    score += 30;
                }
                else if (IsSystemProcess(write))
                {
                    score += 10;
                }

                if (timeDifference <= 1)
                    score += 20;
                else if (timeDifference <= 3)
                    score += 10;
                else
                    score += 5;

                if (score < 60)
                    continue;

                if (bestCandidate == null ||
                    score > bestCandidate.Score)
                {
                    bestCandidate =
                        new ProbableTransferCandidate
                        {
                            Read = read,
                            Write = write,
                            Score = score
                        };
                }
            }

            if (bestCandidate == null)
                continue;

            string sourcePath =
                bestCandidate.Read.Path;

            string targetPath =
                bestCandidate.Write.Path;

            bool alreadyExists = results.Any(x =>
                string.Equals(
                    x.SourcePath,
                    sourcePath,
                    StringComparison.OrdinalIgnoreCase) &&
                string.Equals(
                    x.TargetPath,
                    targetPath,
                    StringComparison.OrdinalIgnoreCase));

            if (alreadyExists)
                continue;

            string processName =
                ResolveDisplayProcessName(
                    bestCandidate.Read,
                    bestCandidate.Write);

            int processId =
                ResolveDisplayProcessId(
                    bestCandidate.Read,
                    bestCandidate.Write);

            results.Add(new ProbableTransferInfo
            {
                ProcessId = processId,
                ProcessName = processName,
                SourcePath = sourcePath,
                TargetPath = targetPath,
                Confidence =
                    GetConfidence(bestCandidate.Score),
                Timestamp =
                    bestCandidate.Read.Timestamp
            });
        }

        return results
            .OrderByDescending(x => x.Timestamp)
            .Take(10)
            .ToList();
    }

    private static string GetConfidence(int score)
    {
        if (score >= 90)
            return "High";

        if (score >= 70)
            return "Medium";

        return "Low";
    }

    private static bool IsSystemProcess(
        FileIoActivity activity)
    {
        return activity.ProcessId == 4 ||
               string.Equals(
                   activity.ProcessName,
                   "System",
                   StringComparison.OrdinalIgnoreCase);
    }

    private static string ResolveDisplayProcessName(
        FileIoActivity read,
        FileIoActivity write)
    {
        if (!IsSystemProcess(read))
            return read.ProcessName;

        if (!IsSystemProcess(write))
            return write.ProcessName;

        return read.ProcessName;
    }

    private static int ResolveDisplayProcessId(
        FileIoActivity read,
        FileIoActivity write)
    {
        if (!IsSystemProcess(read))
            return read.ProcessId;

        if (!IsSystemProcess(write))
            return write.ProcessId;

        return read.ProcessId;
    }

    private static string GetDrive(string path)
    {
        try
        {
            return Path.GetPathRoot(path) ?? "";
        }
        catch
        {
            return "";
        }
    }

    private static string GetFileNameSafe(string path)
    {
        try
        {
            return Path.GetFileName(path);
        }
        catch
        {
            return "";
        }
    }

    private sealed class ProbableTransferCandidate
    {
        public FileIoActivity Read { get; set; } = new();
        public FileIoActivity Write { get; set; } = new();

        public int Score { get; set; }
    }
}