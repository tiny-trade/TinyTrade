using Microsoft.Extensions.Logging;
using System.Diagnostics;

namespace TinyTrade.Services;

internal class LiveService
{
    private const string LiveExecutable = "TinyTrade.Live.exe";
    private readonly ILogger logger;

    public LiveService(ILoggerProvider loggerProvider)
    {
        logger = loggerProvider.CreateLogger(string.Empty);
    }

    public async Task RunLive(string mode, string strategyFile, string pair)
    {
        if (!File.Exists(strategyFile))
        {
            logger.LogError("Unable to locate strategy file {s}", strategyFile);
            return;
        }
        var startInfo = new ProcessStartInfo
        {
            FileName = LiveExecutable,
            Arguments = $"{mode} {strategyFile} {pair}",
            RedirectStandardOutput = true
        };

        //DEBUG
        //startInfo.UserName = null;
        //startInfo.RedirectStandardOutput = false;
        //startInfo.UseShellExecute = true;
        //startInfo.WindowStyle = ProcessWindowStyle.Normal;

        try
        {
            var res = Process.Start(startInfo);
            if (res is not null)
            {
                res.EnableRaisingEvents = true;
                logger.LogInformation("Launched live process [{pid}] in {m} mode with {s} on {p}", res.Id, mode, strategyFile, pair);
            }
            else
            {
                logger.LogError("Unable to launch process");
            }
        }
        catch (Exception ex)
        {
            logger.LogError("{e}", ex);
        }

        await Task.CompletedTask;
    }
}