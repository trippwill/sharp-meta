using SharpMeta;
using Xunit;

namespace Tests.TypeNameInfoTests;

public class From
{
    [Fact]
    public void NullType_ThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() => TypeNameInfo.From(null!));
    }

    [Fact]
    public void GenericType_ReturnsOpenGeneric()
    {
        Type type = typeof(Dictionary<,>);
        var typeNameInfo = TypeNameInfo.From(type);

        Assert.IsType<TypeNameInfo.OpenGeneric>(typeNameInfo);
        Assert.Equal(type.FullName, ((TypeNameInfo.OpenGeneric)typeNameInfo).FullName);
        Assert.Equal(type.Name, typeNameInfo.ShortName);
    }

    [Fact]
    public void ClosedGenericType_ReturnsClosedGeneric()
    {
        Type type = typeof(Dictionary<string, int>);
        var typeNameInfo = TypeNameInfo.From(type);

        Assert.IsType<TypeNameInfo.ClosedGeneric>(typeNameInfo);
        Assert.Equal(type.FullName, ((TypeNameInfo.ClosedGeneric)typeNameInfo).FullName);
        Assert.Equal(type.Name, typeNameInfo.ShortName);
    }

    [Fact]
    public void ArrayType_ReturnsArray()
    {
        Type type = typeof(int[]);
        var typeNameInfo = TypeNameInfo.From(type);

        Assert.IsType<TypeNameInfo.Array>(typeNameInfo);
        Assert.Equal(type.FullName, ((TypeNameInfo.Array)typeNameInfo).FullName);
        Assert.Equal(type.Name, typeNameInfo.ShortName);
    }

    [Fact]
    public void ByRefType_ReturnsByRef()
    {
        Type type = typeof(int).MakeByRefType();
        var typeNameInfo = TypeNameInfo.From(type);

        Assert.IsType<TypeNameInfo.ByRef>(typeNameInfo);
        Assert.Equal(type.FullName, ((TypeNameInfo.ByRef)typeNameInfo).FullName);
        Assert.Equal(type.Name, typeNameInfo.ShortName);
    }

    [Fact]
    public void PointerType_ReturnsPointer()
    {
        Type type = typeof(int).MakePointerType();
        var typeNameInfo = TypeNameInfo.From(type);

        Assert.IsType<TypeNameInfo.Pointer>(typeNameInfo);
        Assert.Equal(type.FullName, ((TypeNameInfo.Pointer)typeNameInfo).FullName);
        Assert.Equal(type.Name, typeNameInfo.ShortName);
    }

    [Fact]
    public void GenericParameterType_ReturnsGenericParameter()
    {
        Type type = typeof(Dictionary<,>).GetGenericArguments()[0];
        var typeNameInfo = TypeNameInfo.From(type);

        Assert.IsType<TypeNameInfo.GenericParameter>(typeNameInfo);
        Assert.Equal(type.Name, typeNameInfo.ShortName);
    }

    [Fact]
    public void DynamicType_ReturnsDynamic()
    {
        var x = new { Hello = "hello" };
        Type type = x.GetType();
        var typeNameInfo = TypeNameInfo.From(type);

        Assert.IsType<TypeNameInfo.Dynamic>(typeNameInfo);
        Assert.Equal(type.Name, typeNameInfo.ShortName);
    }

    [Fact]
    public void StandardType_ReturnsStandard()
    {
        Type type = typeof(int);
        var typeNameInfo = TypeNameInfo.From(type);

        Assert.IsType<TypeNameInfo.Standard>(typeNameInfo);
        Assert.Equal(type.FullName, ((TypeNameInfo.Standard)typeNameInfo).FullName);
        Assert.Equal(type.Name, typeNameInfo.ShortName);
    }
}

public class GetShortName
{
    [Theory]
    [InlineData(typeof(int), NameStyle.Framework, "Int32")]
    [InlineData(typeof(int), NameStyle.DocId, "Int32")]
    [InlineData(typeof(Dictionary<,>), NameStyle.Framework, "Dictionary`2")]
    [InlineData(typeof(Dictionary<,>), NameStyle.DocIdParameter, "Dictionary{``0,``1}")]
    [InlineData(typeof(Dictionary<,>), NameStyle.DocId, "Dictionary`2")]
    [InlineData(typeof(Dictionary<string, int>), NameStyle.Framework, "Dictionary`2")]
    [InlineData(typeof(Dictionary<string, int>), NameStyle.DocIdParameter, "Dictionary{System.String,System.Int32}")]
    [InlineData(typeof(Dictionary<string, int>), NameStyle.DocId, "Dictionary`2")]
    [InlineData(typeof(int[]), NameStyle.Framework, "Int32[]")]
    [InlineData(typeof(int[]), NameStyle.DocId, "Int32[]")]
    [InlineData(typeof(int[,]), NameStyle.Framework, "Int32[,]")]
    [InlineData(typeof(int[,]), NameStyle.DocId, "Int32[0:,0:]")]
    [InlineData(typeof(object), NameStyle.Framework, "Object")]
    [InlineData(typeof(object), NameStyle.DocId, "Object")]
    public void ReturnsExpected(Type type, NameStyle style, string expected)
    {
        var typeNameInfo = TypeNameInfo.From(type);
        var shortName = typeNameInfo.GetShortName(style);
        Assert.Equal(expected, shortName);
    }

    [Theory]
    [InlineData(NameStyle.Framework, "TKey")]
    [InlineData(NameStyle.DocId, "``0")]
    public void ReturnsExpected_ForGenericParameter(NameStyle nameStyle, string expected)
    {
        Type type = typeof(Dictionary<,>).GetGenericArguments()[0];
        var typeNameInfo = TypeNameInfo.From(type);
        var shortName = typeNameInfo.GetShortName(nameStyle);
        Assert.Equal(expected, shortName);
    }

    [Theory]
    [InlineData(NameStyle.Framework, "<>f__AnonymousType")]
    [InlineData(NameStyle.DocId, "<>f__AnonymousType")]
    public void ReturnsExpected_ForDynamicType(NameStyle nameStyle, string expected)
    {
        var x = new { Hello = "hello" };
        Type type = x.GetType();
        var typeNameInfo = TypeNameInfo.From(type);
        var shortName = typeNameInfo.GetShortName(nameStyle);
        Assert.StartsWith(expected, shortName);
    }
}

public class GetFullName
{
    [Theory]
    [InlineData(typeof(int), NameStyle.Framework, "System.Int32")]
    [InlineData(typeof(int), NameStyle.DocId, "System.Int32")]
    [InlineData(typeof(Dictionary<,>), NameStyle.DocIdParameter, "System.Collections.Generic.Dictionary{``0,``1}")]
    [InlineData(typeof(Dictionary<,>), NameStyle.DocId, "System.Collections.Generic.Dictionary`2")]
    [InlineData(typeof(Dictionary<string, int>), NameStyle.DocIdParameter, "System.Collections.Generic.Dictionary{System.String,System.Int32}")]
    [InlineData(typeof(Dictionary<string, int>), NameStyle.DocId, "System.Collections.Generic.Dictionary`2")]
    [InlineData(typeof(int[]), NameStyle.Framework, "System.Int32[]")]
    [InlineData(typeof(int[]), NameStyle.DocId, "System.Int32[]")]
    [InlineData(typeof(int[,]), NameStyle.Framework, "System.Int32[,]")]
    [InlineData(typeof(int[,]), NameStyle.DocId, "System.Int32[0:,0:]")]
    [InlineData(typeof(object), NameStyle.Framework, "System.Object")]
    [InlineData(typeof(object), NameStyle.DocId, "System.Object")]
    public void ReturnsExpected(Type type, NameStyle style, string expected)
    {
        var typeNameInfo = TypeNameInfo.From(type);
        var fullName = typeNameInfo.GetFullName(style);
        Assert.Equal(expected, fullName);
    }

    [Theory]
    [InlineData(NameStyle.Framework, "TKey")]
    [InlineData(NameStyle.DocId, "``0")]
    public void ReturnsExpected_ForGenericParameter(NameStyle nameStyle, string expected)
    {
        Type type = typeof(Dictionary<,>).GetGenericArguments()[0];
        var typeNameInfo = TypeNameInfo.From(type);
        var fullName = typeNameInfo.GetFullName(nameStyle);
        Assert.Equal(expected, fullName);
    }

    [Theory]
    [InlineData(NameStyle.Framework, "<>f__AnonymousType")]
    [InlineData(NameStyle.DocId, "<>f__AnonymousType")]
    public void ReturnsExpected_ForDynamicType(NameStyle nameStyle, string expected)
    {
        var x = new { Hello = "hello" };
        Type type = x.GetType();
        var typeNameInfo = TypeNameInfo.From(type);
        var fullName = typeNameInfo.GetFullName(nameStyle);
        Assert.StartsWith(expected, fullName);
    }
}

public class SpecialTypes
{
    public enum SpecialType
    {
        ByRef,
        Pointer,
    }

    [Theory]
    [InlineData(typeof(int), SpecialType.ByRef, NameStyle.Framework, "Int32&")]
    [InlineData(typeof(int), SpecialType.ByRef, NameStyle.DocId, "Int32@")]
    [InlineData(typeof(int), SpecialType.Pointer, NameStyle.Framework, "Int32*")]
    [InlineData(typeof(int), SpecialType.Pointer, NameStyle.DocId, "Int32*")]
    public void ReturnsExpectedShortName_ForSpecialTypes(Type type, SpecialType special, NameStyle style, string expected)
    {
        Type x = special switch
        {
            SpecialType.ByRef => type.MakeByRefType(),
            SpecialType.Pointer => type.MakePointerType(),
            _ => throw new ArgumentOutOfRangeException(nameof(special))
        };
        var typeNameInfo = TypeNameInfo.From(x);
        var shortName = typeNameInfo.GetShortName(style);
        Assert.Equal(expected, shortName);
        if (special == SpecialType.ByRef)
        {
            Assert.IsType<TypeNameInfo.ByRef>(typeNameInfo);
        }
        else if (special == SpecialType.Pointer)
        {
            Assert.IsType<TypeNameInfo.Pointer>(typeNameInfo);
        }
    }

    [Theory]
    [InlineData(typeof(int), SpecialType.ByRef, NameStyle.Framework, "System.Int32&")]
    [InlineData(typeof(int), SpecialType.ByRef, NameStyle.DocId, "System.Int32@")]
    [InlineData(typeof(int), SpecialType.Pointer, NameStyle.Framework, "System.Int32*")]
    [InlineData(typeof(int), SpecialType.Pointer, NameStyle.DocId, "System.Int32*")]
    public void ReturnsExpectedFullName_ForSpecialTypes(Type type, SpecialType special, NameStyle style, string expected)
    {
        Type x = special switch
        {
            SpecialType.ByRef => type.MakeByRefType(),
            SpecialType.Pointer => type.MakePointerType(),
            _ => throw new ArgumentOutOfRangeException(nameof(special))
        };

        var typeNameInfo = TypeNameInfo.From(x);
        var fullName = typeNameInfo.GetFullName(style);
        Assert.Equal(expected, fullName);
        if (special == SpecialType.ByRef)
        {
            Assert.IsType<TypeNameInfo.ByRef>(typeNameInfo);
        }
        else if (special == SpecialType.Pointer)
        {
            Assert.IsType<TypeNameInfo.Pointer>(typeNameInfo);
        }
    }
}