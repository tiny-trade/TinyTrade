namespace TinyTrade.Core.Strategy;

internal interface IStrategy
{
    void UpdateState(DataFrame frame);

    void OnStart();

    void OnStop();
}