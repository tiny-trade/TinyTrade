using TinyTrade.Core.Strategy;

namespace TinyTrade.Core.Statics;

public static class StrategyResolver
{
    /// <summary>
    /// Automatically resolve a strategy by its class name and obtain the instance, if possible
    /// </summary>
    /// <param name="strategyName"></param>
    /// <param name="parameters"></param>
    /// <param name="strategy"></param>
    /// <returns></returns>
    public static bool TryResolveStrategy(string strategyName, StrategyConstructorParameters parameters, out IStrategy strategy)
    {
        strategy = null!;
        var assemblies = AppDomain.CurrentDomain.GetAssemblies();
        if (assemblies is null || assemblies.Length < 1) return false;
        foreach (var a in assemblies)
        {
            var t = a.ExportedTypes.FirstOrDefault(t => t.Name.Equals(strategyName));
            if (t is not null)
            {
                if (Activator.CreateInstance(t, parameters) is not IStrategy handle) return false;
                strategy = handle;
                return true;
            }
        }
        return false;
    }
}