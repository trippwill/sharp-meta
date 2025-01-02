namespace SharpMeta.Tests;

public class SampleClassWithoutDocs
{
    public int? NullableProperty { get; set; }

    public object SampleMethod() => throw new NotImplementedException();

    [Obsolete("This method is obsolete")]
    public void ObsoleteMethod() { }
}
