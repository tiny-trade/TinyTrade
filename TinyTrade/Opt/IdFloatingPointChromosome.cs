using GeneticSharp;

namespace TinyTrade.Opt;

/// <summary>
///   Decorator of <see cref="FloatingPointChromosome"/> in order to allow parallel evaluation
/// </summary>
internal class IdFloatingPointChromosome : FloatingPointChromosome
{
    private readonly double[] minValue;
    private readonly double[] maxValue;
    private readonly int[] totalBits;
    private readonly int[] fractionDigits;

    public Guid Id { get; }

    public IdFloatingPointChromosome(Guid id, double[] minValue, double[] maxValue, int[] totalBits, int[] fractionDigits, double[]? genes)
                : base(minValue, maxValue, totalBits, fractionDigits, genes)
    {
        Id = id;
        this.minValue = minValue;
        this.maxValue = maxValue;
        this.totalBits = totalBits;
        this.fractionDigits = fractionDigits;
    }

    public override IChromosome CreateNew() => new IdFloatingPointChromosome(Guid.NewGuid(), minValue, maxValue, totalBits, fractionDigits, null);
}