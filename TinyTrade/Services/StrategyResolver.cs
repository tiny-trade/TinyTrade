using System.Reflection;
using TinyTrade.Core.Strategy;

namespace TinyTrade.Services;

internal class StrategyResolver : IStrategyResolver
{
    private const string Namespace = "TinyTrade.Strategies.";

    public StrategyResolver()
    {
    }

    public bool TryResolveStrategy(string strategyName, StrategyConstructorParameters parameters, out IStrategy strategy)
    {
        strategy = null!;
        var assembly = Assembly.GetExecutingAssembly();
        if (assembly is null)
        {
            return false;
        }
        var strategyType = Type.GetType(Namespace + strategyName);
        if (strategyType is null) return false;
        if (Activator.CreateInstance(strategyType, parameters) is not IStrategy handle) return false;
        strategy = handle;
        return true;
    }
}