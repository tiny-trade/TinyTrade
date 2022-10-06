using TinyTrade.Core.Constructs;

namespace TinyTrade.Core.DataProviders;

public class KucoinDataframeProvider : IDataframeProvider
{
    public KucoinDataframeProvider()
    {
    }

    public Task Load()
    {
        // Load everything here. Subscribe to WebSockets if necessary and make REST calls if needed
        return Task.CompletedTask;
    }

    public Task<DataFrame?> Next()
    {
        // Return the next available dataframe. Other services will wait here, so call can be blocking. For example, look at the
        // BacktestDataframeProvider, how the logic is implemented. In that case the dataframes are all available at once, so the Next
        // method simply iterates over them and returns them
        // - New data is received
        // - Convert it into Dataframe
        // - Update the state and "unlock" this function

        // This is just for compile purposes
        return new Task<DataFrame?>(() => null);
    }
}