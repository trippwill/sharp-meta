using System.Collections;
using SharpMeta;
using Xunit;

namespace Tests.TypeExtensionsTest;

public class TryUnwrapNullable
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
}

public class IsProbableDictionary
{
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

public class ImplementsInterface
{
    [Fact]
    public void ImplementsInterface_ShouldReturnTrue_WhenTypeImplementsSpecifiedInterface()
    {
        Type type = typeof(List<int>);
        bool result = type.ImplementsInterface<IList>();
        Assert.True(result);
    }
}

public class ImplementsAnyInterface
{
    [Fact]
    public void ImplementsAnyInterface_ShouldReturnFalse_WhenTypeDoesNotImplementSpecifiedInterface()
    {
        Type type = typeof(List<int>);
        bool result = type.ImplementsInterface<IDisposable>();
        Assert.False(result);
    }

    [Fact]
    public void ImplementsAnyInterface_ShouldReturnTrue_WhenBaseTypeImplementsSpecifiedInterface()
    {
        Type type = typeof(Dictionary<int, string>);
        bool result = type.ImplementsInterface<IDictionary>();
        Assert.True(result);
    }

    [Fact]
    public void ImplementsAnyInterface_ShouldReturnFalse_WhenTypeIsNull()
    {
        Type? type = null;
        Assert.Throws<ArgumentNullException>(() => type!.ImplementsAnyInterface(typeof(IEnumerable<int>).FullName));
    }
}