namespace TinyTrade.Core.Constructs;

/// <summary>
///   Base class representing a generic trait
/// </summary>
[Serializable]
public class Trait
{
    public string Key { get; private set; } = null!;

    public float? Value { get; private set; }

    public Trait(string key, float? value)
    {
        Key = key;
        Value = value;
    }
}