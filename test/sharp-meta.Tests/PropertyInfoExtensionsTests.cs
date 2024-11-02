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
        System.Reflection.PropertyInfo? property = typeof(TestClass).GetProperty(nameof(TestClass.NullableValueType));
        bool result = property!.IsNullable();
        Assert.True(result);
    }

    [Fact]
    public void IsNullable_ShouldReturnFalse_ForNonNullableValueType()
    {
        System.Reflection.PropertyInfo? property = typeof(TestClass).GetProperty(nameof(TestClass.NonNullableValueType));
        bool result = property!.IsNullable();
        Assert.False(result);
    }

    [Fact]
    public void IsNullable_ShouldReturnTrue_ForNullableReferenceType()
    {
        System.Reflection.PropertyInfo? property = typeof(TestClass).GetProperty(nameof(TestClass.NullableReferenceType));
        bool result = property!.IsNullable();
        Assert.True(result);
    }

    [Fact]
    public void IsNullable_ShouldReturnFalse_ForNonNullableReferenceType()
    {
        System.Reflection.PropertyInfo? property = typeof(TestClass).GetProperty(nameof(TestClass.NonNullableReferenceType));
        bool result = property!.IsNullable();
        Assert.False(result);
    }

    [Fact]
    public void IsNullableReference_ShouldReturnTrue_ForNullableReferenceType()
    {
        System.Reflection.PropertyInfo? property = typeof(TestClass).GetProperty(nameof(TestClass.NullableReferenceType));
        bool result = property!.IsNullableReference();
        Assert.True(result);
    }

    [Fact]
    public void IsNullableReference_ShouldReturnFalse_ForNonNullableReferenceType()
    {
        System.Reflection.PropertyInfo? property = typeof(TestClass).GetProperty(nameof(TestClass.NonNullableReferenceType));
        bool result = property!.IsNullableReference();
        Assert.False(result);
    }
}
