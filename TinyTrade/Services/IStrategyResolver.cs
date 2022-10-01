using TinyTrade.Strategies;

namespace TinyTrade.Services;

internal interface IStrategyResolver
{
    bool ResolveStrategy(string strategyName, out IStrategy? strategy);
}