namespace TinyTrade.Strategies;

internal abstract class AbstractStrategy : IStrategy
{
    public void OnStart() => throw new NotImplementedException();

    public void OnStop() => throw new NotImplementedException();

    public void OnTradeOpened() => throw new NotImplementedException();

    public void UpdateState() => throw new NotImplementedException();
}