using HandierCli.CLI;

namespace TinyTrade.Core.Shared;

public static class ArgumentsHandlerFactory
{
    public static ArgumentsHandler.Builder ForRun() => ArgumentsHandler.Factory()
                .Mandatory("mode", new string[] { "foretest", "live" })
                .Mandatory("strategy file", @".json$")
                .Mandatory("pair symbol", @"[A-Z]+-USDT$")
                .Mandatory("exchange", new string[] { "kucoin" });

    public static ArgumentsHandler.Builder ForBacktest() => ArgumentsHandler.Factory()
                .Mandatory("strategy file", @".json$")
                .Mandatory("interval pattern", @"20[1-2][0-9]-[0-1][0-9]|20[1-2][0-9]-[0-1][0-9]")
                .Mandatory("pair symbol", @"[A-Z]+-USDT$");

    public static ArgumentsHandler.Builder ForSnap() => ArgumentsHandler.Factory();

    public static ArgumentsHandler.Builder ForOptimize() => ArgumentsHandler.Factory()
                .Mandatory("strategy file", @".json$")
                .Mandatory("interval pattern", @"20[1-2][0-9]-[0-1][0-9]|20[1-2][0-9]-[0-1][0-9]")
                .Mandatory("pair symbol", @"[A-Z]+-USDT$");
}