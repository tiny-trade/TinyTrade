namespace TinyTrade.Opt.Genes;

internal class FloatGene : StrategyGene
{
    public FloatGene(string key, float value, (float min, float max) bounds) :
        base(key, value, bounds, GeneType.Float)
    {
    }
}