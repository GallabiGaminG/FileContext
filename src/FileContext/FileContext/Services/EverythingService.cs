using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;

namespace FileContext.Services;

public static class EverythingService
{
    private static string GetEsExecutable()
    {
        return Path.Combine(
            AppContext.BaseDirectory,
            "tools",
            "es.exe");
    }

    public static bool IsEsAvailable()
    {
        return File.Exists(GetEsExecutable());
    }

    public static async Task<bool> IsEverythingAvailableAsync()
    {
        if (!IsEsAvailable())
        {
            return false;
        }

        try
        {
            ProcessStartInfo startInfo = new()
            {
                FileName = GetEsExecutable(),
                Arguments = "-n 1 *",
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            using Process process = new()
            {
                StartInfo = startInfo
            };

            process.Start();

            await process.WaitForExitAsync();

            return process.ExitCode == 0;
        }
        catch
        {
            return false;
        }
    }

    public static async Task<List<string>> SearchAsync(string query)
    {
        List<string> results = new();

        if (string.IsNullOrWhiteSpace(query))
        {
            return results;
        }

        ProcessStartInfo startInfo = new()
        {
            FileName = GetEsExecutable(),
            Arguments = $"\"{query}\"",
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true
        };

        using Process process = new()
        {
            StartInfo = startInfo
        };

        process.Start();

        while (!process.StandardOutput.EndOfStream)
        {
            string? line = await process.StandardOutput.ReadLineAsync();

            if (!string.IsNullOrWhiteSpace(line))
            {
                results.Add(line);
            }
        }

        await process.WaitForExitAsync();

        return results;
    }

    private static readonly string[] CommonEverythingPaths =
    {
        Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles),
            "Everything",
            "Everything.exe"),

        Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86),
            "Everything",
            "Everything.exe"),

        @"C:\Program Files\Programlar\Tools\Everything\Everything.exe"
    };

    public static string? FindEverythingExecutable()
    {
        foreach (string path in CommonEverythingPaths)
        {
            if (File.Exists(path))
            {
                return path;
            }
        }

        return null;
    }

    public static async Task<bool> EnsureEverythingRunningAsync()
    {
        if (await IsEverythingAvailableAsync())
        {
            return true;
        }

        string? everythingExe =
            FindEverythingExecutable();

        if (everythingExe == null)
        {
            return false;
        }

        try
        {
            Process.Start(new ProcessStartInfo
            {
                FileName = everythingExe,
                Arguments = "-startup",
                UseShellExecute = true
            });

            for (int i = 0; i < 10; i++)
            {
                await Task.Delay(300);

                if (await IsEverythingAvailableAsync())
                {
                    return true;
                }
            }
        }
        catch
        {
            return false;
        }

        return false;
    }
}