using Xunit;

namespace SharpMeta.Tests;

public class CustomAttributeDataExtensionsTests
{
    [Fact]
    public void GetConstructorArgument_ForSystemAttribute_ShouldReturnCorrectValue()
    {
        var member = typeof(SampleClass).GetMethod(nameof(SampleClass.ObsoleteMethod));
        member.TryGetCustomAttributeData<ObsoleteAttribute>(out var attributeData);
        var message = attributeData.GetConstructorArgument<string>(0);
        Assert.Equal("This method is obsolete", message);
    }

    [Fact]
    public void GetNamedArgument_ShouldReturnCorrectValue()
    {
        typeof(SampleClass).TryGetCustomAttributeData<SampleAttribute>(out var attributeData);
        var value = attributeData.GetNamedArgument<string>("Name");
        Assert.Equal("Test", value);
    }

    [Fact]
    public void TryGetNamedArgument_ShouldReturnTrueAndCorrectValue()
    {
        typeof(SampleClass).TryGetCustomAttributeData<SampleAttribute>(out var attributeData);
        var result = attributeData.TryGetNamedArgument<string>("Name", out var value);
        Assert.True(result);
        Assert.Equal("Test", value);
    }

    [Fact]
    public void GetConstructorArgument_ShouldReturnCorrectValue()
    {
        typeof(SampleClass).TryGetCustomAttributeData<SampleAttribute>(out var attributeData);
        var value = attributeData.GetConstructorArgument<int>(0);
        Assert.Equal(42, value);
    }

    [Fact]
    public void TryGetConstructorArgument_ShouldReturnTrueAndCorrectValue()
    {
        typeof(SampleClass).TryGetCustomAttributeData<SampleAttribute>(out var attributeData);
        var result = attributeData.TryGetConstructorArgument<int>(0, out var value);
        Assert.True(result);
        Assert.Equal(42, value);
    }
}
