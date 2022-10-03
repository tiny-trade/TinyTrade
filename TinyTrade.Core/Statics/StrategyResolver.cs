using TinyTrade.Core.Strategy;

namespace TinyTrade.Core.Statics;

public static class StrategyResolver
{
    private const string Namespace = "TinyTrade.Strategies.";

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