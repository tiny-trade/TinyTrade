using TinyTrade.Core.Strategy;

namespace TinyTrade.Services;

internal interface IStrategyResolver
{
    bool TryResolveStrategy(string strategyName, StrategyConstructorParameters parameters, out IStrategy strategy);
}