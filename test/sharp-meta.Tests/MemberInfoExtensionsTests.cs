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
    public void TryGetCustomAttributeData_FromMemberInfoAndFullName_ShouldReturnTrue()
    {
        MethodInfo? member = typeof(SampleClass).GetMethod(nameof(SampleClass.SampleMethod));
        bool result = member!.TryGetCustomAttributeData("SharpMeta.Tests.SampleAttribute", out CustomAttributeData? attributeData);
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

    [Fact]
    public void GetAllCustomAttributeData_ShouldReturnAttributesForProperty()
    {
        PropertyInfo? property = typeof(SampleClass).GetProperty(nameof(SampleClass.AttributedProperty));
        IList<CustomAttributeData>? attributes = property!.GetAllCustomAttributeData(includeInherited: false);
        Assert.NotNull(attributes);
        Assert.Contains(attributes, a => a.AttributeType == typeof(SampleAttribute));
    }

    [Fact]
    public void GetAllCustomAttributeData_ShouldIncludeInheritedAttributesForProperty()
    {
        PropertyInfo? property = typeof(DerivedSampleClass).GetProperty(nameof(DerivedSampleClass.AttributedProperty));
        IList<CustomAttributeData>? attributes = property!.GetAllCustomAttributeData(includeInherited: true);
        Assert.NotNull(attributes);
        Assert.Contains(attributes, a => a.AttributeType == typeof(SampleAttribute));
    }

    [Fact]
    public void TryGetDocComments_ShouldReturnFalse_WhenMemberInfoIsNull()
    {
        MemberInfo? memberInfo = null;
        bool result = memberInfo!.TryGetDocComments(out DocComments? docComments);
        Assert.False(result);
        Assert.Null(docComments);
    }

    [Fact]
    public void TryGetDocComments_ShouldReturnFalse_WhenMemberNotFoundInXmlDocumentation()
    {
        Type type = typeof(SampleClassWithoutDocs);
        bool result = type.TryGetDocComments(out DocComments? docComments);
        Assert.False(result);
        Assert.Null(docComments);
    }

    [Fact]
    public void TryGetDocComments_ShouldReturnTrue_WhenMemberFoundInXmlDocumentation()
    {
        Type type = typeof(SampleClass);
        bool result = type.TryGetDocComments(out DocComments? docComments);
        Assert.True(result);
        Assert.NotNull(docComments);
        Assert.Equal("A sample class.", docComments?.Summary);
    }

    [Fact]
    public void TryGetDocComments_ShouldReturnTrue_ForMethod()
    {
        MethodInfo? method = typeof(SampleClass).GetMethod(nameof(SampleClass.SampleMethod));
        bool result = method!.TryGetDocComments(out DocComments? docComments);
        Assert.True(result);
        Assert.NotNull(docComments);
        Assert.Equal("A sample method.", docComments?.Summary);
    }

    [Fact]
    public void TryGetDocComments_ShouldReturnTrue_ForProperty()
    {
        PropertyInfo? property = typeof(SampleClass).GetProperty(nameof(SampleClass.NullableProperty));
        bool result = property!.TryGetDocComments(out DocComments? docComments);
        Assert.True(result);
        Assert.NotNull(docComments);
        Assert.Equal("Gets or sets a nullable property.", docComments?.Summary);
        Assert.Empty(docComments!.Parameters);
    }

    [Fact]
    public void TryGetDocComments_ShouldReturnTrue_ForObsoleteMethod()
    {
        MethodInfo? method = typeof(SampleClass).GetMethod(nameof(SampleClass.ObsoleteMethod));
        bool result = method!.TryGetDocComments(out DocComments? docComments);
        Assert.True(result);
        Assert.NotNull(docComments);
        Assert.Equal("An obsolete method.", docComments?.Summary);
        Assert.Empty(docComments!.Parameters);
    }
}
