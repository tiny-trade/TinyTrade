using TinyTrade.Core.Constructs;

namespace TinyTrade.Core.Strategy;

public interface IStrategy
{
    void UpdateState(DataFrame frame);

    void OnStart();

    void OnStop();
}