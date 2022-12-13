using Microsoft.Extensions.Logging;
using System.Diagnostics;
using System.Text.RegularExpressions;

namespace TinyTrade.Services;

internal class SnapService
{
    private const string LiveAssemblyName = "TinyTrade.Live";
    private readonly ILogger logger;
    private readonly Regex? liveAssemblyRegex;

    public SnapService(ILoggerProvider loggerProvider)
    {
        logger = loggerProvider.CreateLogger(string.Empty);

        liveAssemblyRegex = new Regex(LiveAssemblyName);
    }

    public void Snapshot()
    {
        try
        {
            if (liveAssemblyRegex is null)
            {
                logger.LogWarning("Unable to define regex for Live process assembly name");
                return;
            }
            var liveProcesses = new List<Process>();
            var processes = Process.GetProcesses();
            foreach (var f in processes)
            {
                if (liveAssemblyRegex.IsMatch(f.ProcessName))
                {
                    liveProcesses.Add(f);
                }
            }
            for (var i = 0; i < liveProcesses.Count; i++)
            {
                var proc = liveProcesses[i];
                logger.LogInformation("{i}: {name} [{pid}]", i, proc.ProcessName, proc.Id);
            }
            if (liveProcesses.Count <= 0)
            {
                logger.LogInformation("No live processes found");
            }
        }
        catch (Exception e)
        {
            logger.LogError("Exception captured: {e}", e.Message);
        }
    }
}