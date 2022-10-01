namespace TinyTrade.Strategies;

internal interface IStrategy
{
    void UpdateState();

    void OnStart();

    void OnStop();

    void OnTradeOpened();
}