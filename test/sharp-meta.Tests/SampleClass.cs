namespace SharpMeta.Tests;

/// <summary>
/// A sample class.
/// </summary>
[Sample(42, Name = "Test")]
public class SampleClass
{
    /// <summary>
    /// Gets or sets a nullable property.
    /// </summary>
    public int? NullableProperty { get; set; }

    [Sample(42)]
    public bool AttributedProperty { get; set; }

    /// <summary>
    /// A sample method.
    /// </summary>
    /// <returns></returns>
    /// <exception cref="NotImplementedException"></exception>
    [Sample(42)]
    public object SampleMethod() => throw new NotImplementedException();

    /// <summary>
    /// An obsolete method.
    /// </summary>
    [Obsolete("This method is obsolete")]
    public void ObsoleteMethod() { }
}
