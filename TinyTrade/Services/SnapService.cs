using Microsoft.Extensions.Logging;

namespace TinyTrade.Services;

internal class SnapService
{
    private readonly ILogger logger;

    public SnapService(ILoggerProvider loggerProvider)
    {
        logger = loggerProvider.CreateLogger(string.Empty);
    }

    public void Snapshot()
    {
        try
        {
        }
        catch (Exception e)
        {
            logger.LogError("Exception captured: {e}", e.Message);
        }
    }
}