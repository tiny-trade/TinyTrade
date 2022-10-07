using Newtonsoft.Json;

namespace TinyTrade.Core.Constructs;

public class DataFrame
{
    public float Open { get; init; }

    public float Close { get; init; }

    public float High { get; init; }

    public float Low { get; init; }

    public float Volume { get; init; }

    public ulong OpenTime { get; init; }

    public ulong CloseTime { get; init; }

    public bool IsClosed { get; set; }

    public DataFrame(ulong openTime, float open, float high, float low, float close, float volume, ulong closeTime, bool isClosed)
    {
        OpenTime = openTime;
        Open = open;
        High = high;
        Low = low;
        Close = close;
        Volume = volume;
        CloseTime = closeTime;
        IsClosed = isClosed;
    }

    public override string? ToString() => JsonConvert.SerializeObject(this, Formatting.Indented);
}