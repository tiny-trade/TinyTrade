using System.Collections.Concurrent;
using TinyTrade.Core.Constructs;

namespace TinyTrade.Core.DataProviders;

public class ParallelBacktestDataframeProvider : BacktestDataframeProvider
{
    private readonly ConcurrentDictionary<Guid, int> indexesMap;

    internal ParallelBacktestDataframeProvider(TimeInterval interval, Pair pair, Timeframe timeframe) : base(interval, pair, timeframe)
    {
        indexesMap = new ConcurrentDictionary<Guid, int>();
    }

    public override async Task<DataFrame?> Next(Guid? identifier = null)
    {
        await Task.CompletedTask;
        if (identifier is null) return null;
        var id = (Guid)identifier;
        if (!indexesMap.ContainsKey(id))
        {
            indexesMap.TryAdd(id, 0);
            return frames[0];
        }
        else
        {
            if (indexesMap.TryGetValue(id, out var index))
            {
                var res = index >= FramesCount ? null : frames[index];
                indexesMap[id] = index + 1;
                return res;
            }
            return null;
        }
    }

    public override void Reset(Guid? identifier = null)
    {
        if (identifier is null) return;
        var id = (Guid)identifier;

        if (indexesMap.TryGetValue(id, out var index))
        {
            indexesMap.TryUpdate(id, 0, index);
        }
    }
}