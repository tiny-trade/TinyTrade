using HandierCli.CLI;
using Microsoft.Extensions.Logging;

namespace TinyTrade.Services.Logging;

/// <summary>
/// The provider of the <see cref="CommandLine.CliLogger"/>
/// </summary>
[ProviderAlias("CliLog")]
internal class CliLoggerProvider : ILoggerProvider
{
    private readonly CommandLine cli;

    public CliLoggerProvider(CommandLine cli)
    {
        this.cli = cli;
    }

    public ILogger CreateLogger(string categoryName) => cli.CliLogger;

    public void Dispose()
    {
    }
}