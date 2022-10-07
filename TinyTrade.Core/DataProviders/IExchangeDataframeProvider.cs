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
        await Load(progress);
        var p = new LoadProgress
        {
            Description = "Preloading candles"
        };
        progress?.Report(p);
        return await PreloadCandles(amount);
    }

    Task<bool> PreloadCandles(int amount);
}