using Microsoft.Extensions.Logging;
using TinyTrade.Core.Exchanges;

namespace TinyTrade.Core.Strategy;

internal struct StrategyConstructorParameters
{
    public Dictionary<string, object> Parameters { get; init; }

    public Dictionary<string, float> Genotype { get; init; }

    public ILogger Logger { get; init; }

    public IExchange Exchange { get; init; }

    public StrategyConstructorParameters(Dictionary<string, object> parameters, Dictionary<string, float> genotype, ILogger logger, IExchange exchange)
    {
        Parameters = parameters;
        Genotype = genotype;
        Logger = logger;
        Exchange = exchange;
    }
}