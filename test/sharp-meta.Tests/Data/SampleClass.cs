namespace Tests.Data;

/// <summary>
/// A sample class.
/// </summary>
[Sample(42, Name = "Test")]
public class SampleClass
{
#pragma warning disable CS0067
    public event EventHandler? SampleEvent;
#pragma warning restore CS0067

    public bool SampleField;

    /// <summary>
    /// Gets or sets a nullable property.
    /// </summary>
    public int? NullableProperty { get; set; }

    /// <summary>
    /// An attributed property.
    /// </summary>
    [Sample(42)]
    public bool AttributedProperty { get; set; }

    /// <summary>
    /// A sample method.
    /// </summary>
    /// <returns>Nothing.</returns>
    /// <exception cref="NotImplementedException"></exception>
    [Sample(42)]
    public object SampleMethod() => throw new NotImplementedException();

    /// <summary>
    /// An obsolete method.
    /// </summary>
    [Obsolete("This method is obsolete")]
    public void ObsoleteMethod() { }

    public object MethodWithoutDocComments() => throw new NotImplementedException();
}