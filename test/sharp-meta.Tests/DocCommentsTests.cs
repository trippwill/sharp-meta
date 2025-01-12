using System.Diagnostics.CodeAnalysis;
using SharpMeta;
using Xunit;

namespace Tests.DocCommentsTests;

[SuppressMessage("Performance", "CA1822:Mark members as static", Justification = "Test Code")]
public class Parse
{
    static Parse()
    {
        DocComments.MaxDepth = 24;
    }

    [Fact]
    public void ShouldThrow_WhenMemberInfoIsNull()
    {
        Assert.Throws<ArgumentNullException>(() => DocComments.Parse(null!));
    }

    [Fact]
    public void ShouldReturnNull_WhenXmlDocumentationIsNotFound()
    {
        var memberInfo = typeof(TestClass).GetMethod(nameof(TestClass.MethodWithoutXmlDocs));
        var result = DocComments.Parse(memberInfo!);
        Assert.Null(result);
    }

    [Fact]
    public void ShouldReturnDocComments_WhenXmlDocumentationIsFound()
    {
        var memberInfo = typeof(TestClass).GetMethod(nameof(TestClass.MethodWithXmlDocs));
        var result = DocComments.Parse(memberInfo!);
        Assert.NotNull(result);
        Assert.Equal("This is a summary.", result!.Summary);
        Assert.Equal("This is a remark.", result.Remarks);
        Assert.Contains("This is an example.", result.Examples);
        Assert.Equal("This is a return value.", result.Returns);
        Assert.Contains(result.Parameters, p => p.Name == "param1" && p.Value == "This is a parameter.");
        Assert.Contains(result.Exceptions, e => e.Name == "System.Exception" && e.Value == "This is an exception.");
    }

    [Fact]
    public void ShouldHandleInheritdoc()
    {
        var memberInfo = typeof(DerivedClass).GetMethod(nameof(DerivedClass.MethodWithInheritdoc));
        var result = DocComments.Parse(memberInfo!);
        Assert.NotNull(result);
        Assert.Equal("Base summary.", result!.Summary);
        Assert.Equal("Base remark.", result.Remarks);
    }

    [Fact]
    public void ShouldHandleMultipleExamples()
    {
        var memberInfo = typeof(TestClass).GetMethod(nameof(TestClass.MethodWithMultipleExamples));
        var result = DocComments.Parse(memberInfo!);
        Assert.NotNull(result);
        Assert.Contains("This is the first example.", result!.Examples);
        Assert.Contains("This is the second example.", result.Examples);
    }

    [Fact]
    public void ShouldHandleInterfaceInheritance()
    {
        var memberInfo = typeof(ImplementingClass).GetMethod(nameof(ImplementingClass.InterfaceMethod));
        var result = DocComments.Parse(memberInfo!);
        Assert.NotNull(result);
        Assert.Equal("Interface method summary.", result!.Summary);
        Assert.Equal("Interface method remark.", result.Remarks);
    }

    [Fact]
    public void ShouldHandleInheritdocWithCref()
    {
        var memberInfo = typeof(DerivedClassWithCref).GetMethod(nameof(DerivedClassWithCref.MethodWithInheritdocCref));
        var result = DocComments.Parse(memberInfo!);
        Assert.NotNull(result);
        Assert.Equal("Referenced summary.", result!.Summary);
        Assert.Equal("Referenced remark.", result.Remarks);
    }
}

public class MaxDepthTests : IDisposable
{
    public void Dispose()
    {
        DocComments.MaxDepth = 24;
    }

    [Fact]
    public void ShouldBeSettable()
    {
        DocComments.MaxDepth = 10;
        Assert.Equal(10, DocComments.MaxDepth);
    }

    [Fact]
    public void ShouldThrow_WhenMaxDepthExceeded()
    {
        DocComments.MaxDepth = 1;
        var memberInfo = typeof(TestClass).GetMethod(nameof(TestClass.MethodWithXmlDocs));
        Assert.Throws<InvalidOperationException>(() => DocComments.Parse(memberInfo!));
    }
}

public class Normalize
{
    [Fact]
    public void ShouldNormalizeStringValues()
    {
        var docComments = new DocComments(
            "<para>This is a summary.</para>",
            "<remarks>This is a remark.<br/>With a line break.</remarks>",
            "<returns>This is a return value.</returns>",
            [("param1", "<param>This is a parameter.</param>")],
            [],
            [],
            ["<example>This is an example.<br />With a line break.</example>"]
        );

        var normalized = docComments.Normalize();

        Assert.Equal("This is a summary.", normalized.Summary);
        Assert.Equal("This is a remark.\nWith a line break.", normalized.Remarks);
        Assert.Equal("This is a return value.", normalized.Returns);
        Assert.Contains(normalized.Parameters, p => p.Name == "param1" && p.Value == "This is a parameter.");
        Assert.Contains(normalized.Examples, e => e == "This is an example.\nWith a line break.");
    }
}

internal class TestClass
{
    /// <summary>This is a summary.</summary>
    /// <remarks>This is a remark.</remarks>
    /// <example>This is an example.</example>
    /// <returns>This is a return value.</returns>
    /// <param name="param1">This is a parameter.</param>
    /// <exception cref="Exception">This is an exception.</exception>
    public void MethodWithXmlDocs(bool param1) { }

    public void MethodWithoutXmlDocs() { }

    /// <summary>This is a summary.</summary>
    /// <remarks>This is a remark.</remarks>
    /// <example>This is the first example.</example>
    /// <example>This is the second example.</example>
    public void MethodWithMultipleExamples() { }
}

internal class BaseClass
{
    /// <summary>Base summary.</summary>
    /// <remarks>Base remark.</remarks>
    public virtual void MethodWithInheritdoc() { }
}

internal class DerivedClass : BaseClass
{
    /// <inheritdoc />
    public override void MethodWithInheritdoc() { }
}

internal interface ITestInterface
{
    /// <summary>Interface method summary.</summary>
    /// <remarks>Interface method remark.</remarks>
    void InterfaceMethod();
}

internal class ImplementingClass : ITestInterface
{
    /// <inheritdoc />
    public void InterfaceMethod() { }
}

internal class BaseClassWithCref
{
    /// <summary>Referenced summary.</summary>
    /// <remarks>Referenced remark.</remarks>
    public void ReferencedMethod() { }
}

internal class DerivedClassWithCref
{
    /// <inheritdoc cref="BaseClassWithCref.ReferencedMethod" />
    public void MethodWithInheritdocCref() { }
}
