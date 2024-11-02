using Xunit;

namespace SharpMeta.Tests;

public class CustomAttributeDataExtensionsTests
{
    [Fact]
    public void GetConstructorArgument_ForSystemAttribute_ShouldReturnCorrectValue()
    {
        System.Reflection.MethodInfo? member = typeof(SampleClass).GetMethod(nameof(SampleClass.ObsoleteMethod));
        member!.TryGetCustomAttributeData<ObsoleteAttribute>(out System.Reflection.CustomAttributeData? attributeData);
        string? message = attributeData!.GetConstructorArgument<string>(0);
        Assert.Equal("This method is obsolete", message);
    }

    [Fact]
    public void GetNamedArgument_ShouldReturnCorrectValue()
    {
        typeof(SampleClass).TryGetCustomAttributeData<SampleAttribute>(out System.Reflection.CustomAttributeData? attributeData);
        string? value = attributeData!.GetNamedArgument<string>("Name");
        Assert.Equal("Test", value);
    }

    [Fact]
    public void TryGetNamedArgument_ShouldReturnTrueAndCorrectValue()
    {
        typeof(SampleClass).TryGetCustomAttributeData<SampleAttribute>(out System.Reflection.CustomAttributeData? attributeData);
        bool result = attributeData!.TryGetNamedArgument<string>("Name", out string? value);
        Assert.True(result);
        Assert.Equal("Test", value);
    }

    [Fact]
    public void GetConstructorArgument_ShouldReturnCorrectValue()
    {
        typeof(SampleClass).TryGetCustomAttributeData<SampleAttribute>(out System.Reflection.CustomAttributeData? attributeData);
        int value = attributeData!.GetConstructorArgument<int>(0);
        Assert.Equal(42, value);
    }

    [Fact]
    public void TryGetConstructorArgument_ShouldReturnTrueAndCorrectValue()
    {
        typeof(SampleClass).TryGetCustomAttributeData<SampleAttribute>(out System.Reflection.CustomAttributeData? attributeData);
        bool result = attributeData!.TryGetConstructorArgument<int>(0, out int value);
        Assert.True(result);
        Assert.Equal(42, value);
    }
}
