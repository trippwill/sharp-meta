using System.Reflection;
using Xunit;

namespace SharpMeta.Tests;

public class MemberInfoExtensionsTests
{
    [Fact]
    public void TryGetCustomAttributeData_FromAttributeDataCollection_ShouldReturnTrue()
    {
        var type = typeof(SampleClass);
        var attributeData = type.GetCustomAttributesData().First(a => a.AttributeType == typeof(SampleAttribute));
        var attributeDataList = new List<CustomAttributeData> { attributeData };
        var result = attributeDataList.TryGetCustomAttributeData<SampleAttribute>(out var foundAttributeData);
        Assert.True(result);
        Assert.NotNull(foundAttributeData);
    }

    [Fact]
    public void TryGetCustomAttributeData_FromMemberInfo_ShouldReturnTrue()
    {
        var member = typeof(SampleClass).GetMethod(nameof(SampleClass.SampleMethod));
        var result = member.TryGetCustomAttributeData<SampleAttribute>(out var attributeData);
        Assert.True(result);
        Assert.NotNull(attributeData);
    }

    [Fact]
    public void TryGetCustomAttributeData_FromMemberInfoAndType_ShouldReturnTrue()
    {
        var member = typeof(SampleClass).GetMethod(nameof(SampleClass.SampleMethod));
        var result = member.TryGetCustomAttributeData(typeof(SampleAttribute), out var attributeData);
        Assert.True(result);
        Assert.NotNull(attributeData);
    }

    [Fact]
    public void TryGetCustomAttributeData_FromMemberInfoAndString_ShouldReturnTrue()
    {
        var member = typeof(SampleClass).GetMethod(nameof(SampleClass.SampleMethod));
        var result = member.TryGetCustomAttributeData(typeof(SampleAttribute).FullName, out var attributeData);
        Assert.True(result);
        Assert.NotNull(attributeData);
    }
}
