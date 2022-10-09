using GeneticSharp;

namespace TinyTrade.Opt.Modules;

internal class IdentifiableFloatingPointChromosome : FloatingPointChromosome
{
    private readonly double[] minValue;
    private readonly double[] maxValue;
    private readonly int[] totalBits;
    private readonly int[] fractionDigits;

    public Guid Id { get; }

    public IdentifiableFloatingPointChromosome(Guid id, double[] minValue, double[] maxValue, int[] totalBits, int[] fractionDigits, double[]? genes)
                : base(minValue, maxValue, totalBits, fractionDigits, genes)
    {
        Id = id;
        this.minValue = minValue;
        this.maxValue = maxValue;
        this.totalBits = totalBits;
        this.fractionDigits = fractionDigits;
    }

    public override IChromosome CreateNew() => new IdentifiableFloatingPointChromosome(Guid.NewGuid(), minValue, maxValue, totalBits, fractionDigits, null);
}