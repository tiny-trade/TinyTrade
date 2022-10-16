using Kucoin.Net.Enums;
using TinyTrade.Core.Constructs;

namespace TinyTrade.Core.Statics;

public static class Extensions
{
    public static async Task<bool> DownloadFile(this HttpClient client, string address, string fileName)
    {
        using var response = await client.GetAsync(address);
        if (response.IsSuccessStatusCode)
        {
            using var stream = await response.Content.ReadAsStreamAsync();
            using var file = File.OpenWrite(fileName);
            stream.CopyTo(file);
            return true;
        }
        return false;
    }

    public static T TraitValueOrDefault<T>(this List<Trait> genes, string key, T defaultVal) where T : notnull
    {
        var g = genes.FirstOrDefault(g => g.Key.Equals(key));
        if (g is not null)
        {
            T? res = (T?)Convert.ChangeType(g.Value, typeof(T));
            if (res is null) return defaultVal;
        }
        return defaultVal;
    }

    public static FuturesKlineInterval ToFuturesInterval(this KlineInterval interval)
    {
        switch (interval)
        {
            case KlineInterval.OneMinute:
                return FuturesKlineInterval.OneMinute;

            case KlineInterval.FiveMinutes:
                return FuturesKlineInterval.FiveMinutes;

            case KlineInterval.FifteenMinutes:
                return FuturesKlineInterval.FifteenMinutes;

            case KlineInterval.ThirtyMinutes:
                return FuturesKlineInterval.ThirtyMinutes;

            case KlineInterval.OneHour:
                return FuturesKlineInterval.OneHour;

            case KlineInterval.TwoHours:
                return FuturesKlineInterval.TwoHours;

            case KlineInterval.FourHours:
                return FuturesKlineInterval.FourHours;

            case KlineInterval.EightHours:
                return FuturesKlineInterval.EightHours;

            case KlineInterval.TwelveHours:
                return FuturesKlineInterval.TwelveHours;

            case KlineInterval.OneDay:
                return FuturesKlineInterval.OneDay;

            case KlineInterval.OneWeek:
                return FuturesKlineInterval.OneWeek;

            default:
                return FuturesKlineInterval.FiveMinutes;
        }
    }
}