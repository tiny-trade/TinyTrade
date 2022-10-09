namespace TinyTrade.Opt.Genes;

internal class IntGene : StrategyGene
{
    public IntGene(string key, int value, (int min, int max) bounds) :
        base(key, value, bounds, GeneType.Integer)
    {
    }
}