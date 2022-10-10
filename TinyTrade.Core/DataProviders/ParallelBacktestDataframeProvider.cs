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
    private readonly ConcurrentDictionary<Guid, Applicant> indexesMap;

    private readonly int batchCount;
    private readonly int[] batchIndexes;

    internal ParallelBacktestDataframeProvider(TimeInterval interval, Pair pair, Timeframe timeframe, int batchCount = 1) : base(interval, pair, timeframe)
    {
        indexesMap = new ConcurrentDictionary<Guid, Applicant>();
        this.batchCount = batchCount;
        batchIndexes = new int[this.batchCount];
    }

    public override async Task Load(IProgress<IDataframeProvider.LoadProgress>? progress = null)
    {
        await base.Load(progress);

        var elementsPerBatch = FramesCount / batchCount;
        for (int i = 0; i < batchCount; i++)
        {
            batchIndexes[i] = i == batchCount - 1 ? FramesCount : elementsPerBatch * (i + 1);
        }
    }

    public bool HasAnotherBatch(Guid identifier) => !indexesMap.TryGetValue(identifier, out var applicant) || applicant.currentBatch < batchCount - 1;

    public override async Task<DataFrame?> Next(Guid? identifier = null)
    {
        await Task.CompletedTask;
        if (identifier is null) return null;
        var id = (Guid)identifier;
        if (!indexesMap.TryGetValue(id, out var applicant))
        {
            indexesMap.TryAdd(id, new Applicant());
            return frames[0];
        }
        else
        {
            if (applicant.currentIndex >= FramesCount) return null;
            if (applicant.currentIndex == batchIndexes[applicant.currentBatch])
            {
                applicant.currentBatch++;
                applicant.currentIndex++;
                return null;
            }
            var res = frames[applicant.currentIndex];
            applicant.currentIndex++;
            return res;
        }
    }

    public void Clear(Guid? identifier = null)
    {
        if (identifier is null) return;
        var id = (Guid)identifier;
        indexesMap.TryRemove(id, out _);
    }

    public override void Reset(Guid? identifier = null)
    {
        if (identifier is null) return;
        var id = (Guid)identifier;
        if (indexesMap.TryGetValue(id, out var applicant))
        {
            applicant.currentBatch = 0;
            applicant.currentIndex = 0;
        }
    }

    private class Applicant
    {
        public int currentIndex;

        public int currentBatch;

        public Applicant()
        {
        }
    }
}