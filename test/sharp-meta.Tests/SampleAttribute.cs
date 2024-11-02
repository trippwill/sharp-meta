namespace SharpMeta.Tests;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method | AttributeTargets.Property)]
public class SampleAttribute(int id) : Attribute
{
    public int Id { get; } = id;

    public string Name { get; set; } = string.Empty;
}
