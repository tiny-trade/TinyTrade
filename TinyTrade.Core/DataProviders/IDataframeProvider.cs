using TinyTrade.Core.Constructs;

namespace TinyTrade.Core.DataProviders;

public interface IDataframeProvider
{
    /// <summary>
    ///   Initialize the provider
    /// </summary>
    /// <returns> </returns>
    Task Load();

    /// <summary>
    ///   Ask and wait for the next available <see cref="DataFrame"/>
    /// </summary>
    /// <returns> </returns>
    Task<DataFrame?> Next();
}