using System.Reflection;
using TinyTrade.Strategies;

namespace TinyTrade.Services;

internal class StrategyResolver : IStrategyResolver
{
    private readonly string namespaceName;

    public StrategyResolver()
    {
        namespaceName = typeof(IStrategy).Namespace ?? string.Empty;
        if (!string.IsNullOrEmpty(namespaceName))
        {
            namespaceName += ".";
        }
    }

    public bool ResolveStrategy(string strategyName, out IStrategy? strategy)
    {
        strategy = null;
        var assembly = Assembly.GetExecutingAssembly();
        if (assembly is null)
        {
            return false;
        }
        var assemblyName = assembly.GetName().FullName;
        var handle = Activator.CreateInstance(assemblyName, namespaceName + strategyName);
        if (handle is null) return false;
        var obj = handle.Unwrap();
        if (obj is not IStrategy) return false;
        strategy = obj as IStrategy;
        return true;
    }
}