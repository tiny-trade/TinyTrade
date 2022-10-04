using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Diagnostics;
using System.Reflection;
using System.Text.RegularExpressions;
using TinyTrade.Statics;

namespace TinyTrade.Services.Hosted;

internal class CleanupHostedService : IHostedService
{
    private readonly ILogger logger;

    public CleanupHostedService(ILoggerProvider loggerProvider)
    {
        logger = loggerProvider.CreateLogger(string.Empty);
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        logger.LogTrace("Cleaning up zombie caches");
        return CleanupZombieCaches();
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;

    private async Task CleanupZombieCaches()
    {
        if (!Directory.Exists(Paths.Processes)) return;
        await Task.Run(() =>
        {
            var files = Directory.GetFiles(Paths.Processes);
            var processes = Process.GetProcesses();
            var assemblyName = Assembly.GetExecutingAssembly().GetName().Name;
            var regex = new Regex($"^{assemblyName}");
            foreach (var f in files)
            {
                var info = new FileInfo(f);
                var pidString = Path.GetFileNameWithoutExtension(info.Name);
                if (info.Extension != ".json" || !int.TryParse(pidString, out var pid))
                {
                    File.Delete(f);
                    continue;
                }
                var p = processes.FirstOrDefault(p => p.Id == pid);
                if (p is null || !regex.IsMatch(p.ProcessName))
                {
                    File.Delete(f);
                }
            }
        });
    }
}