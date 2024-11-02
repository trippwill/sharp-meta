using Xunit;

namespace SharpMeta.Tests;

public class TypeExtensionsTests
{
    [Fact]
    public void TryUnwrapNullable_ShouldReturnTrue_WhenNullableType()
    {
        Type type = typeof(int?);
        bool result = type.TryUnwrapNullable(out Type? underlyingType);
        Assert.True(result);
        Assert.Equal(typeof(int), underlyingType);
    }

    [Fact]
    public void TryUnwrapNullable_ShouldReturnFalse_WhenNonNullableType()
    {
        Type type = typeof(int);
        bool result = type.TryUnwrapNullable(out Type? underlyingType);
        Assert.False(result);
        Assert.Null(underlyingType);
    }

    [Fact]
    public void TryUnwrapNullable_ShouldThrowArgumentNullException_WhenTypeIsNull()
    {
        Type? type = null;
        Assert.Throws<ArgumentNullException>(() => type!.TryUnwrapNullable(out _));
    }

    [Fact]
    public void IsProbableDictionary_ShouldReturnTrue()
    {
        Type type = typeof(Dictionary<int, string>);
        bool result = type.IsProbableDictionary(out Type? keyType, out Type? valueType);
        Assert.True(result);
        Assert.Equal(typeof(int), keyType);
        Assert.Equal(typeof(string), valueType);
    }
}
