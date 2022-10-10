using TinyTrade.Core.Constructs;

namespace TinyTrade.Core.Strategy;

public interface IStrategy
{
    Task UpdateState(DataFrame frame);

    void OnStart();

    void Reset();

    void OnStop();
}