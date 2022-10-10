namespace TinyTrade.Core.DataProviders;

public interface IExchangeDataframeProvider : IDataframeProvider
{
    /// <summary>
    ///   Load the provider and preload the specified amount of klines into the strategy.
    ///   <para> NOTE: either call this function or <see cref="IDataframeProvider.Load"/>, but not both </para>
    /// </summary>
    /// <param name="amount"> </param>
    /// <returns> </returns>
    async Task<bool> LoadAndPreloadCandles(int amount, IProgress<LoadProgress>? progress = null)
    {
        var p = new LoadProgress
        {
            Description = "Loading dataframe provider"
        };
        progress?.Report(p);
        await Load(progress);
        p.Description = "Preloading candles";
        progress?.Report(p);
        var res = await PreloadCandles(amount);
        p.Description = res ? "Candles preloaded" : "Unable to preload candles";
        p.Progress = 1F;
        progress?.Report(p);
        return res;
    }

    Task<bool> PreloadCandles(int amount);
}