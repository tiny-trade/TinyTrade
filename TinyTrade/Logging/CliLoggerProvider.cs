using HandierCli;
using Microsoft.Extensions.Logging;

namespace TinyTrade.Logging;

[ProviderAlias("CliLog")]
internal class CliLoggerProvider : ILoggerProvider
{
    private readonly CommandLine cli;

    public CliLoggerProvider(CommandLine cli)
    {
        this.cli = cli;
    }

    public ILogger CreateLogger(string categoryName) => cli.Logger;

    public void Dispose()
    {
    }
}