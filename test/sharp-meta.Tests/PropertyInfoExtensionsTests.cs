using Xunit;

namespace SharpMeta.Tests;

public class PropertyInfoExtensionsTests
{
    private class TestClass
    {
        public int NonNullableValueType { get; set; }
        public int? NullableValueType { get; set; }
        public string NonNullableReferenceType { get; set; } = string.Empty;
        public string? NullableReferenceType { get; set; }
    }

    [Fact]
    public void IsNullable_ShouldReturnTrue_ForNullableValueType()
    {
        var property = typeof(TestClass).GetProperty(nameof(TestClass.NullableValueType));
        var result = property.IsNullable();
        Assert.True(result);
    }

    [Fact]
    public void IsNullable_ShouldReturnFalse_ForNonNullableValueType()
    {
        var property = typeof(TestClass).GetProperty(nameof(TestClass.NonNullableValueType));
        var result = property.IsNullable();
        Assert.False(result);
    }

    [Fact]
    public void IsNullable_ShouldReturnTrue_ForNullableReferenceType()
    {
        var property = typeof(TestClass).GetProperty(nameof(TestClass.NullableReferenceType));
        var result = property.IsNullable();
        Assert.True(result);
    }

    [Fact]
    public void IsNullable_ShouldReturnFalse_ForNonNullableReferenceType()
    {
        var property = typeof(TestClass).GetProperty(nameof(TestClass.NonNullableReferenceType));
        var result = property.IsNullable();
        Assert.False(result);
    }

    [Fact]
    public void IsNullableReference_ShouldReturnTrue_ForNullableReferenceType()
    {
        var property = typeof(TestClass).GetProperty(nameof(TestClass.NullableReferenceType));
        var result = property.IsNullableReference();
        Assert.True(result);
    }

    [Fact]
    public void IsNullableReference_ShouldReturnFalse_ForNonNullableReferenceType()
    {
        var property = typeof(TestClass).GetProperty(nameof(TestClass.NonNullableReferenceType));
        var result = property.IsNullableReference();
        Assert.False(result);
    }
}
