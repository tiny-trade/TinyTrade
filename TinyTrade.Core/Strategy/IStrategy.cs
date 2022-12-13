using TinyTrade.Core.Constructs;

namespace TinyTrade.Core.Strategy;

/// <summary>
/// Basic interface for a strategy. It is highly recommende to inherit the already implemented <see cref="AbstractStrategy"/> 
/// unless strictly necessary for completely new strategies
/// </summary>
public interface IStrategy
{
    /// <summary>
    ///   Update the internal state of the strategy
    /// </summary>
    /// <returns> </returns>
    Task UpdateState(DataFrame frame);

    /// <summary>
    ///   Reset the status of all conditions of the strategy
    /// </summary>
    void Reset();
}