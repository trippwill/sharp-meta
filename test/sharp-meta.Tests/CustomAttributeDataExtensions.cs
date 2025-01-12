using SharpMeta;
using Tests.Data;
using Xunit;

namespace Tests.CustomAttributeDataExtensions;

public class GetConstructorArgument
{
    [Fact]
    public void ForSystemAttribute_ShouldReturnCorrectValue()
    {
        System.Reflection.MethodInfo? member = typeof(SampleClass).GetMethod(nameof(SampleClass.ObsoleteMethod));
        member!.TryGetCustomAttributeData<ObsoleteAttribute>(out System.Reflection.CustomAttributeData? attributeData);
        string? message = attributeData!.GetConstructorArgument<string>(0);
        Assert.Equal("This method is obsolete", message);
    }

    [Fact]
    public void ShouldReturnCorrectValue()
    {
        typeof(SampleClass).TryGetCustomAttributeData<SampleAttribute>(out System.Reflection.CustomAttributeData? attributeData);
        int value = attributeData!.GetConstructorArgument<int>(0);
        Assert.Equal(42, value);
    }
}

public class GetNamedArgument
{
    [Fact]
    public void ShouldReturnCorrectValue()
    {
        typeof(SampleClass).TryGetCustomAttributeData<SampleAttribute>(out System.Reflection.CustomAttributeData? attributeData);
        string? value = attributeData!.GetNamedArgument<string>("Name");
        Assert.Equal("Test", value);
    }
}

public class TryGetConstructorArgument
{
    [Fact]
    public void ShouldReturnCorrectValue()
    {
        typeof(SampleClass).TryGetCustomAttributeData<SampleAttribute>(out System.Reflection.CustomAttributeData? attributeData);
        bool result = attributeData!.TryGetConstructorArgument<int>(0, out int value);
        Assert.True(result);
        Assert.Equal(42, value);
    }
}

public class TryGetNamedArgument
{
    [Fact]
    public void ShouldReturnCorrectValue()
    {
        typeof(SampleClass).TryGetCustomAttributeData<SampleAttribute>(out System.Reflection.CustomAttributeData? attributeData);
        bool result = attributeData!.TryGetNamedArgument<string>("Name", out string? value);
        Assert.True(result);
        Assert.Equal("Test", value);
    }
}