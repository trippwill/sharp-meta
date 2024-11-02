namespace SharpMeta.Tests;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method | AttributeTargets.Property)]
public class SampleAttribute : Attribute
{
    public SampleAttribute(int id)
    {
        Id = id;
    }

    public int Id { get; }

    public string Name { get; set; }
}
