using CryptoExchange.Net.Objects;
using CryptoExchange.Net.Sockets;
using Kucoin.Net.Clients;
using Kucoin.Net.Enums;
using Kucoin.Net.Objects.Models.Spot;
using Kucoin.Net.Objects.Models.Spot.Socket;
using TinyTrade.Core.Constructs;
using static TinyTrade.Core.DataProviders.IDataframeProvider;

namespace TinyTrade.Core.DataProviders;

public class KucoinDataframeProvider : IExchangeDataframeProvider
{
    private readonly KucoinSocketClient socketClient;
    private readonly KucoinClient client;
    private readonly Pair pair;
    private readonly KlineInterval klineInterval;
    private readonly Queue<DataFrame> dataFrames;
    private readonly Timeframe timeframe;
    private DataFrame? oldCandle;

    internal KucoinDataframeProvider(Pair pair, Timeframe timeframe)
    {
        this.pair = pair;
        this.timeframe = timeframe;
        klineInterval = IntervalConverter(timeframe);
        socketClient = new KucoinSocketClient();
        client = new KucoinClient();
        dataFrames = new Queue<DataFrame>();
        oldCandle = null;
    }

    public async Task Load(IProgress<LoadProgress>? progress = null) => _ = await socketClient.SpotStreams.SubscribeToKlineUpdatesAsync(pair.ForKucoin(), klineInterval, Callback);

    public async Task<DataFrame?> Next(Guid? identifier = null)
    {
        while (dataFrames.Count <= 0)
        {
            await Task.Delay(200);
        }
        return dataFrames.Dequeue();
    }

    public async Task<bool> PreloadCandles(int amount)
    {
        if (amount <= 0) return true;
        var ratio = (double)amount / 200;
        var tasksNumber = (int)Math.Ceiling(ratio);
        var tasks = new Task<WebCallResult<IEnumerable<KucoinKline>>>[tasksNumber];
        var now = new DateTimeOffset(DateTime.Now.ToUniversalTime());
        var to = now;
        for (int i = 0; i < tasksNumber; i++)
        {
            var add = (int)(Math.Min(ratio, 1) * 200F);
            var from = to.AddSeconds(-(timeframe.Minutes * 60) * add);
            tasks[i] = client.SpotApi.ExchangeData.GetKlinesAsync(pair.ForKucoin(), klineInterval, startTime: from.DateTime, endTime: to.DateTime);
            ratio -= 1;
            to = from;
            if (ratio <= 0) break;
        }
        await Task.WhenAll(tasks);
        bool success = true;
        foreach (var t in tasks)
        {
            if (!t.Result.Success)
            {
                success = false;
                return false;
            }
        }
        if (!success)
        {
            return false;
        }

        for (var i = 0; i < tasks.Length; i++)
        {
            var reversed = tasks[i].Result.Data.Reverse();
            foreach (var k in reversed)
            {
                dataFrames.Enqueue(Convert(k, true));
            }
        }
        return true;
    }

    public void Reset(Guid? identifier = null)
    { }

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
        var newCandle = Convert(obj.Data.Candles);
        if (oldCandle is not null)
        {
            if (newCandle.OpenTime != oldCandle.OpenTime)
            {
                oldCandle.IsClosed = true;
            }
            dataFrames.Enqueue(oldCandle);
        }

        oldCandle = newCandle;
    }

    private DataFrame Convert(KucoinKline candle, bool forceClosed = false)
    {
        var openTime = (ulong)new DateTimeOffset(candle.OpenTime).ToUnixTimeSeconds();
        var open = (float)candle.OpenPrice;
        var high = (float)candle.HighPrice;
        var low = (float)candle.LowPrice;
        var close = (float)candle.ClosePrice;
        var volume = (float)candle.Volume;
        var closeTime = openTime + (ulong)klineInterval;

        return new DataFrame(openTime, open, high, low, close, volume, closeTime, forceClosed);
    }
}