using Microsoft.Extensions.Logging;
using System.Diagnostics;

namespace TinyTrade.Services;

internal class LiveService
{
    private const string LiveExecutable = "./TinyTrade.Live.exe";
    private readonly ILogger logger;

    public LiveService(ILoggerProvider loggerProvider)
    {
        logger = loggerProvider.CreateLogger(string.Empty);
    }

    public async Task RunLive(string mode, string strategyFile, string pair)
    {
        var process = new Process
        {
            StartInfo = new ProcessStartInfo
            {
                FileName = LiveExecutable,
                Arguments = $"{mode} {strategyFile} {pair}",
                UseShellExecute = false,
                RedirectStandardOutput = true,
                CreateNoWindow = true
            }
        };
        var res = process.Start();
        if (res)
        {
            logger.LogInformation("Launched live process [{pid}] in {m} mode with {s} on {p}", process.Id, mode, strategyFile, pair);
        }
        else
        {
            logger.LogError("Unable to launch process");
        }
        await Task.CompletedTask;
    }
}