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
        MethodInfo? method = typeof(DocClass).GetMethod(nameof(DocClass.ObsoleteMethod));
        bool result = method!.TryGetDocComments(out DocComments? docComments);
        Assert.True(result);
        Assert.NotNull(docComments);
        Assert.Equal("An obsolete method.", docComments?.Summary);
        Assert.Empty(docComments!.Parameters);
    }

    [Theory]
    [InlineData(typeof(SampleClass), "A sample class.")]
    [InlineData(typeof(DocClassWithRemarks), "A sample class with remarks.")]
    public void GetDocComments_ShouldReturnCorrectSummary_ForClass(Type type, string expectedSummary)
    {
        DocComments? docComments = type.GetDocComments();
        Assert.NotNull(docComments);
        Assert.Equal(expectedSummary, docComments?.Summary);
    }

    [Theory]
    [InlineData(nameof(SampleClass.SampleMethod), "A sample method.")]
    [InlineData(nameof(SampleClass.ObsoleteMethod), "An obsolete method.")]
    public void GetDocComments_ShouldReturnCorrectSummary_ForMethod(string methodName, string expectedSummary)
    {
        MethodInfo? method = typeof(SampleClass).GetMethod(methodName);
        DocComments? docComments = method!.GetDocComments();
        Assert.NotNull(docComments);
        Assert.Equal(expectedSummary, docComments?.Summary);
    }

    [Theory]
    [InlineData(nameof(SampleClass.NullableProperty), "Gets or sets a nullable property.")]
    [InlineData(nameof(SampleClass.AttributedProperty), "An attributed property.")]
    public void GetDocComments_ShouldReturnCorrectSummary_ForProperty(string propertyName, string expectedSummary)
    {
        PropertyInfo? property = typeof(SampleClass).GetProperty(propertyName);
        DocComments? docComments = property!.GetDocComments();
        Assert.NotNull(docComments);
        Assert.Equal(expectedSummary, docComments?.Summary);
    }

    [Fact]
    public void GetDocComments_ShouldReturnCorrectRemarks()
    {
        Type type = typeof(DocClassWithRemarks);
        DocComments? docComments = type.GetDocComments();
        Assert.NotNull(docComments);
        Assert.Equal("Some remarks about the sample class.", docComments?.Remarks);
    }

    [Fact]
    public void GetDocComments_ShouldReturnCorrectExample()
    {
        Type type = typeof(DocClassWithExample);
        DocComments? docComments = type.GetDocComments();
        Assert.NotNull(docComments);
        Assert.Equal("An example of using the sample class.", docComments?.Example);
    }

    [Fact]
    public void GetDocComments_ShouldReturnCorrectReturns()
    {
        MethodInfo? method = typeof(DocClass).GetMethod(nameof(DocClass.SampleMethodWithReturn));
        DocComments? docComments = method!.GetDocComments();
        Assert.NotNull(docComments);
        Assert.Equal("The result of the sample method.", docComments?.Returns);
    }

    [Fact]
    public void GetDocComments_ShouldReturnCorrectParameters()
    {
        MethodInfo? method = typeof(DocClass).GetMethod(nameof(DocClass.SampleMethodWithParameters));
        DocComments? docComments = method!.GetDocComments();
        Assert.NotNull(docComments);
        Assert.Equal(2, docComments!.Parameters.Length);
        Assert.Contains(docComments.Parameters, p => p.Name == "param1" && p.Value == "The first parameter.");
        Assert.Contains(docComments.Parameters, p => p.Name == "param2" && p.Value == "The second parameter.");
    }

    [Fact]
    public void GetDocComments_ShouldReturnCorrectTypeParameters()
    {
        MethodInfo? method = typeof(DocClass).GetMethod(nameof(DocClass.SampleGenericMethod));
        DocComments? docComments = method!.GetDocComments();
        Assert.NotNull(docComments);
        Assert.Single(docComments!.TypeParameters);
        Assert.Contains(docComments.TypeParameters, tp => tp.Name == "T" && tp.Value == "The type parameter.");
    }

    [Fact]
    public void GetDocComments_ShouldReturnCorrectExceptions()
    {
        MethodInfo? method = typeof(DocClass).GetMethod(nameof(DocClass.SampleMethodWithException));
        DocComments? docComments = method!.GetDocComments();
        Assert.NotNull(docComments);
        Assert.Single(docComments!.Exceptions);
        Assert.Contains(docComments.Exceptions, e => e.Name == "System.Exception" && e.Value == "An exception that can be thrown.");
    }

    // Sample classes and methods for testing
    public class DocClass
    {
        /// <summary>A sample class.</summary>
        public DocClass() { }

        /// <summary>A sample method.</summary>
        public void SampleMethod() { }

        /// <summary>Gets or sets a nullable property.</summary>
        public int? NullableProperty { get; set; }

        /// <summary>An attributed property.</summary>
        [Sample(42)]
        public string? AttributedProperty { get; set; }

        /// <summary>An obsolete method.</summary>
        [Obsolete]
        public void ObsoleteMethod() { }

        /// <returns>The result of the sample method.</returns>
        public int SampleMethodWithReturn() => 0;

        /// <summary>
        /// A sample method with parameters.
        /// </summary>
        /// <param name="param1">The first parameter.</param>
        /// <param name="param2">The second parameter.</param>
        public void SampleMethodWithParameters(int param1, string param2) { }

        /// <summary>
        /// A sample generic method.
        /// </summary>
        /// <typeparam name="T">The type parameter.</typeparam>
        public void SampleGenericMethod<T>() { }

        /// <summary>
        /// A sample method with an exception.
        /// </summary>
        /// <exception cref="System.Exception">An exception that can be thrown.</exception>
        public static void SampleMethodWithException() { }

        /// <summary>
        /// A sample method with an array parameter.
        /// </summary>
        /// <param name="array">The array parameter.</param>
        public void SampleMethodWithArrayParameter(int[] array) { }

        /// <summary>
        /// A sample method with a pointer parameter.
        /// </summary>
        /// <param name="pointer">The pointer parameter.</param>
        public unsafe void SampleMethodWithPointerParameter(int* pointer) { }

        /// <summary>
        /// A sample method with a by-ref parameter.
        /// </summary>
        /// <param name="byRef">The by-ref parameter.</param>
        public void SampleMethodWithByRefParameter(ref int byRef) { }
    }

    /// <summary>A sample class with remarks.</summary>
    /// <remarks>Some remarks about the sample class.</remarks>
    public class DocClassWithRemarks { }

    /// <summary>A sample class with an example.</summary>
    /// <example>An example of using the sample class.</example>
    public class DocClassWithExample { }
}
