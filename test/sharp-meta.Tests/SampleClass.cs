
namespace SharpMeta.Tests;

[Sample(42, Name = "Test")]
public class SampleClass
{
    public int? NullableProperty { get; set; }

    [Sample(42)]
    public static object SampleMethod() => throw new NotImplementedException();

    [Obsolete("This method is obsolete")]
    public void ObsoleteMethod() { }
}
