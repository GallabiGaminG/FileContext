using System.Diagnostics;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace FileContext.Services;

public static class EverythingService
{
    private const string EsExecutable =
    @"C:\Program Files\Programlar\Tools\Everything\ES-1.1.0.38.x64\es.exe";

    public static async Task<List<string>> SearchAsync(string query)
    {
        List<string> results = new();

        if (string.IsNullOrWhiteSpace(query))
        {
            return results;
        }

        ProcessStartInfo startInfo = new()
        {
            FileName = EsExecutable,
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