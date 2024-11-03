using System.Reflection;
using Xunit;

namespace SharpMeta.Tests;

public class MemberInfoExtensionsTests
{
    [Fact]
    public void TryGetCustomAttributeData_FromAttributeDataCollection_ShouldReturnTrue()
    {
        Type type = typeof(SampleClass);
        CustomAttributeData attributeData = type.GetCustomAttributesData().First(a => a.AttributeType == typeof(SampleAttribute));
        var attributeDataList = new List<CustomAttributeData> { attributeData };
        bool result = attributeDataList.TryGetCustomAttributeData<SampleAttribute>(out CustomAttributeData? foundAttributeData);
        Assert.True(result);
        Assert.NotNull(foundAttributeData);
    }

    [Fact]
    public void TryGetCustomAttributeData_FromMemberInfo_ShouldReturnTrue()
    {
        MethodInfo? member = typeof(SampleClass).GetMethod(nameof(SampleClass.SampleMethod));
        bool result = member!.TryGetCustomAttributeData<SampleAttribute>(out CustomAttributeData? attributeData);
        Assert.True(result);
        Assert.NotNull(attributeData);
    }

    [Fact]
    public void TryGetCustomAttributeData_FromMemberInfoAndType_ShouldReturnTrue()
    {
        MethodInfo? member = typeof(SampleClass).GetMethod(nameof(SampleClass.SampleMethod));
        bool result = member!.TryGetCustomAttributeData(typeof(SampleAttribute), out CustomAttributeData? attributeData);
        Assert.True(result);
        Assert.NotNull(attributeData);
    }

    [Fact]
    public void GetAllCustomAttributeData_ShouldReturnAllAttributes()
    {
        Type type = typeof(SampleClass);
        IList<CustomAttributeData>? attributes = type.GetAllCustomAttributeData(includeInherited: false);
        Assert.NotNull(attributes);
        Assert.Contains(attributes, a => a.AttributeType == typeof(SampleAttribute));
    }

    [Fact]
    public void GetAllCustomAttributeData_ShouldIncludeInheritedAttributes()
    {
        Type type = typeof(DerivedSampleClass);
        IList<CustomAttributeData>? attributes = type.GetAllCustomAttributeData(includeInherited: true);
        Assert.NotNull(attributes);
        Assert.Contains(attributes, a => a.AttributeType == typeof(SampleAttribute));
    }

    [Fact]
    public void GetAllCustomAttributeData_ShouldReturnAttributesForMember()
    {
        MethodInfo? member = typeof(SampleClass).GetMethod(nameof(SampleClass.SampleMethod));
        IList<CustomAttributeData>? attributes = member!.GetAllCustomAttributeData(includeInherited: false);
        Assert.NotNull(attributes);
        Assert.Contains(attributes, a => a.AttributeType == typeof(SampleAttribute));
    }

    [Fact]
    public void GetAllCustomAttributeData_ShouldIncludeInheritedAttributesForMember()
    {
        MethodInfo? member = typeof(DerivedSampleClass).GetMethod(nameof(DerivedSampleClass.SampleMethod));
        IList<CustomAttributeData>? attributes = member!.GetAllCustomAttributeData(includeInherited: true);
        Assert.NotNull(attributes);
        Assert.Contains(attributes, a => a.AttributeType == typeof(SampleAttribute));
    }
}
