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

    public LiveProcessModel(int pid, string mode, string strategy, string pair)
    {
        Pid = pid;
        Mode = mode;
        Strategy = strategy;
        Pair = pair;
    }
}