using Microsoft.Extensions.Logging;
using System.Diagnostics;
using System.Runtime.InteropServices;
using TinyTrade.Core.Shared;

namespace TinyTrade.Services;

internal class RunService
{
    private const string LiveExecutable = "TinyTrade.Live";
    private readonly ILogger logger;

    public RunService(ILoggerProvider loggerProvider)
    {
        logger = loggerProvider.CreateLogger(string.Empty);
    }

    public async Task RunLive(RunMode mode, Exchange exchange, string strategyFile, string pair)
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
                Arguments = $"{mode.ToString().ToLower()} {strategyFile} {pair} {exchange.ToString().ToLower()}",
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
                res.Exited += (s, a) => RunProcessTerminated(res);
                logger.LogInformation("Launched live process: [{pid}]\nMode {m}\nStrategy: {s}\nPair: {p}\nExchange {e}", res.Id, mode, strategyFile, pair, exchange);
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

    private void RunProcessTerminated(Process process)
    {
        logger.LogWarning("Run process {pid} has terminated with exit code {c}", process.Id, process.ExitCode);
    }

    private string GetOsSuffix() => RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? ".exe" : string.Empty;
}