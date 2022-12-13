using TinyTrade.Core.Shared;

namespace TinyTrade.Live.Models;

internal class LiveProcessModel
{
    public int Pid { get; private set; }

    public RunMode Mode { get; init; }

    public Exchange Exchange { get; init; }

    public string Strategy { get; init; } = null!;

    public string Pair { get; init; } = null!;

    public float Balance { get; init; }

    public float Withdrawn { get; init; }

    public int OpenPositions { get; init; }

    public LiveProcessModel(int pid, RunMode mode, string strategy, string pair, float withdrawn, float balance, int openPosition)
    {
        Withdrawn = withdrawn;
        Pid = pid;
        Mode = mode;
        Strategy = strategy;
        Pair = pair;
        Balance = balance;
        OpenPositions = openPosition;
    }
}