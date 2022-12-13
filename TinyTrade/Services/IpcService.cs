using Microsoft.Extensions.Logging;
using System.IO.Pipes;

namespace TinyTrade.Services;

/// <summary>
/// Service to perform inter process communication
/// </summary>
internal class IpcService
{
    private readonly ILogger logger;

    public IpcService(ILoggerFactory loggerFactory)
    {
        logger = loggerFactory.CreateLogger(string.Empty);
    }

    /// <summary>
    /// Send the specified data to the process with the specified pid
    /// </summary>
    /// <param name="pid"></param>
    /// <param name="message"></param>
    /// <returns></returns>
    public async Task SendAsync(int pid, string message, int timeout = 2000)
    {
        try
        {
            var pipe = new NamedPipeClientStream($"{pid}.pipe");
            var writer = new StreamWriter(pipe);
            await pipe.ConnectAsync(timeout);
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