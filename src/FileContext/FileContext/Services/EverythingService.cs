using System;
using System.Diagnostics;
using System.IO;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace FileContext.Services;

public static class EverythingService
{
    private static string GetEsExecutable()
    {
        string? configuredPath =
            Environment.GetEnvironmentVariable("FILECONTEXT_ES_PATH");

        if (!string.IsNullOrWhiteSpace(configuredPath) &&
            File.Exists(configuredPath))
        {
            return configuredPath;
        }

        return "es.exe";
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
}