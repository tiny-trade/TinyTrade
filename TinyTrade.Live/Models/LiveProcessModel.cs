using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using TinyTrade.Core.Shared;

namespace TinyTrade.Live.Models;

internal class LiveProcessModel
{
    [JsonProperty("pid")]
    public int Pid { get; private set; }

    [JsonConverter(typeof(StringEnumConverter))]
    [JsonProperty("mode")]
    public RunMode Mode { get; init; }

    [JsonConverter(typeof(StringEnumConverter))]
    [JsonProperty("exchange")]
    public Exchange Exchange { get; init; }

    [JsonProperty("strategy")]
    public string Strategy { get; init; } = null!;

    [JsonProperty("pair")]
    public string Pair { get; init; } = null!;

    [JsonProperty("balance")]
    public float Balance { get; init; }

    [JsonProperty("openPositions")]
    public int OpenPositions { get; init; }

    public LiveProcessModel(int pid, RunMode mode, Exchange exchange, string strategy, string pair, float balance, int openPosition)
    {
        Pid = pid;
        Mode = mode;
        Strategy = strategy;
        Pair = pair;
        Balance = balance;
        OpenPositions = openPosition;
    }
}