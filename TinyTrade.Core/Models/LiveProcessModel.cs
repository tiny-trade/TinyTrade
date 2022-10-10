using CryptoExchange.Net.CommonObjects;
using Newtonsoft.Json;

namespace TinyTrade.Core.Models;

public class LiveProcessModel
{
    [JsonProperty("pid")]
    public int Pid { get; private set; }

    [JsonProperty("mode")]
    public string Mode { get; init; } = null!;

    [JsonProperty("strategy")]
    public string Strategy { get; init; } = null!;

    [JsonProperty("pair")]
    public string Pair { get; init; } = null!;

    [JsonProperty("balance")]
    public float Balance { get; init; }

    [JsonProperty("openPositions")]
    public int OpenPositions { get; init; }

    public LiveProcessModel(int pid, string mode, string strategy, string pair, float balance, int openPosition)
    {
        Pid = pid;
        Mode = mode;
        Strategy = strategy;
        Pair = pair;
        Balance = balance;
        OpenPositions = openPosition;
    }
}