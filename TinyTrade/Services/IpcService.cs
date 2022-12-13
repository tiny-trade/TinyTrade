using Microsoft.Extensions.Logging;
using System.IO.Pipes;

namespace TinyTrade.Services;

internal class IpcService
{
    private readonly ILogger logger;

    public IpcService(ILoggerFactory loggerFactory)
    {
        logger = loggerFactory.CreateLogger(string.Empty);
    }

    public async Task SendAsync(int pid, string message)
    {
        try
        {
            var pipe = new NamedPipeClientStream($"{pid}.pipe");
            var writer = new StreamWriter(pipe);
            await pipe.ConnectAsync(2000);
            await writer.WriteLineAsync(message);
            await writer.FlushAsync();
            await pipe.FlushAsync();
            writer.Close();
            pipe.Close();
            logger.LogInformation("Wrote {m} to {p}", message, pid);
        }
        catch (Exception ex)
        {
            logger.LogWarning("Unable to establish IPC communication: {e}", ex.Message);
        }
    }
}