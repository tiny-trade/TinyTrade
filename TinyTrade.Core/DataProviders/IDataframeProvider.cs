using TinyTrade.Core.Constructs;

namespace TinyTrade.Core.DataProviders;

public interface IDataframeProvider
{
    void Reset(Guid? identifier = null);

    /// <summary>
    ///   Initialize the provider
    /// </summary>
    /// <returns> </returns>
    Task Load(IProgress<LoadProgress>? progress = null);

    /// <summary>
    ///   Ask and wait for the next available <see cref="DataFrame"/>
    /// </summary>
    /// <returns> </returns>
    Task<DataFrame?> Next(Guid? identifier = null);

    public struct LoadProgress
    {
        public float Progress { get; set; }

        public int Step { get; set; }

        public int TotalSteps { get; set; }

        public string Description { get; set; }
    }
}