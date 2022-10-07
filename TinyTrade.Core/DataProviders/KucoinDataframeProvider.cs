using CryptoExchange.Net.CommonObjects;
using CryptoExchange.Net.Sockets;
using Kucoin.Net.Clients;
using Kucoin.Net.Enums;
using Kucoin.Net.Objects.Models.Spot.Socket;
using TinyTrade.Core.Constructs;

namespace TinyTrade.Core.DataProviders;

public class KucoinDataframeProvider : IDataframeProvider
{
    private readonly KucoinSocketClient client;
    private readonly string symbol;
    private readonly KlineInterval interval;
    private readonly Queue<DataFrame> dataFrames;
    private DataFrame? oldCandle;

    public KucoinDataframeProvider(string symbol, Timeframe interval)
    {
        int ind = symbol.IndexOf("USDT");
        string token = symbol[..ind];
        this.symbol = token + "-" + "USDT";
        this.interval = IntervalConverter(interval);
        client = new KucoinSocketClient();
        dataFrames = new Queue<DataFrame>();
        oldCandle = null;
    }

    public async Task Load() => _ = await client.SpotStreams.SubscribeToKlineUpdatesAsync(symbol, interval, Callback);

    public async Task<DataFrame?> Next()
    {
        while (dataFrames.Count <= 0)
        {
            await Task.Delay(200);
        }
        return dataFrames.Dequeue();
    }

    private static KlineInterval IntervalConverter(Timeframe timeframe)
    {
        var k = (KlineInterval)(timeframe.Minutes * 60);
        if (!Enum.IsDefined(typeof(KlineInterval), k))
        {
            k = KlineInterval.FiveMinutes;
        }
        return k;
    }

    private void Callback(DataEvent<KucoinStreamCandle> obj)
    {
        var openTime = (ulong)new DateTimeOffset(obj.Data.Candles.OpenTime).ToUnixTimeSeconds();
        var open = (float)obj.Data.Candles.OpenPrice;
        var high = (float)obj.Data.Candles.HighPrice;
        var low = (float)obj.Data.Candles.LowPrice;
        var close = (float)obj.Data.Candles.ClosePrice;
        var volume = (float)obj.Data.Candles.Volume;
        var closeTime = (ulong)openTime + (ulong)interval;

        if (oldCandle is not null)
        {
            if (openTime != oldCandle.OpenTime)
            {
                oldCandle.IsClosed = true;
            }
            dataFrames.Enqueue(oldCandle);
        }

        oldCandle = new DataFrame(openTime, open, high, low, close, volume, closeTime, false);
    }
}