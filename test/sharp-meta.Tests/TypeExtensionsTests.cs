using Xunit;

namespace SharpMeta.Tests;

public class TypeExtensionsTests
{
    [Fact]
    public void TryUnwrapNullable_ShouldReturnTrue_WhenNullableType()
    {
        var type = typeof(int?);
        var result = type.TryUnwrapNullable(out var underlyingType);
        Assert.True(result);
        Assert.Equal(typeof(int), underlyingType);
    }

    [Fact]
    public void TryUnwrapNullable_ShouldReturnFalse_WhenNonNullableType()
    {
        var type = typeof(int);
        var result = type.TryUnwrapNullable(out var underlyingType);
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
        var type = typeof(Dictionary<int, string>);
        var result = type.IsProbableDictionary(out var keyType, out var valueType);
        Assert.True(result);
        Assert.Equal(typeof(int), keyType);
        Assert.Equal(typeof(string), valueType);
    }
}
