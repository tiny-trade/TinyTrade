using System.Collections.Concurrent;
using TinyTrade.Core.Constructs;

namespace TinyTrade.Core.DataProviders;

/// <summary>
///   Extension of <see cref="BacktestDataframeProvider"/> that allows to parallel evaluate strategies by identifying them through <see
///   cref="Guid"/>. The class keeps a <see cref="ConcurrentDictionary{TKey, TValue}"/> of guids and frames indexes. Before a new evaluation
///   call <see cref="Reset(Guid?)"/> to reset the frame index corresponding to the specified identifier. In order to prevent memory leaks
///   or worst, after running the evaluation, remember to call <see cref="Clear(Guid?)"/> in order to remove the guid from the dictionary,
///   preventing it to grow indefinetly. In this way, the dictionary is assured to always stay updated.
/// </summary>
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
                indexesMap.TryUpdate(id, index + 1, index);
                return res;
            }
            return null;
        }
    }

    public void Clear(Guid? identifier = null)
    {
        if (identifier is null) return;
        var id = (Guid)identifier;
        _ = indexesMap.Remove(id, out _);
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