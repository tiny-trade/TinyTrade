using Microsoft.Extensions.Logging;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace TinyTrade.Services;

internal class RunService
{
    private const string LiveExecutable = "TinyTrade.Live";
    private readonly ILogger logger;

    public RunService(ILoggerProvider loggerProvider)
    {
        logger = loggerProvider.CreateLogger(string.Empty);
    }

    public async Task RunLive(string mode, string strategyFile, string pair)
    {
        await Task.CompletedTask;
        try
        {
            if (!File.Exists(strategyFile))
            {
                logger.LogError("Unable to locate strategy file {s}", strategyFile);
                return;
            }
            var startInfo = new ProcessStartInfo
            {
                FileName = LiveExecutable + GetOsSuffix(),
                Arguments = $"{mode} {strategyFile} {pair}",
                RedirectStandardOutput = true,
                UserName = null,
            };
            startInfo.RedirectStandardOutput = false;
            startInfo.UseShellExecute = true;
            startInfo.WindowStyle = ProcessWindowStyle.Hidden;
            //DEBUG
            //startInfo.WindowStyle = ProcessWindowStyle.Normal;

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
        catch (Exception e)
        {
            logger.LogError("Exception captured: {e}", e.Message);
        }
    }

    private string GetOsSuffix() => RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? ".exe" : string.Empty;
}