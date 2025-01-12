using System.Reflection;
using SharpMeta;
using Tests.Data;
using Xunit;
using Xunit.Abstractions;

namespace Tests.MemberInfoExtensionsTests;

public class GetDocId
{
    private readonly ITestOutputHelper _output;
    private static System.Xml.XmlDocument? _xmlDoc;

    public GetDocId(ITestOutputHelper output)
    {
        _output = output;
        LoadXmlDoc();
    }

    private static void LoadXmlDoc()
    {
        if (_xmlDoc is null)
        {
            string docFile = Path.ChangeExtension(typeof(MyClass).Assembly.Location, ".xml");
            if (System.IO.File.Exists(docFile))
            {
                _xmlDoc = new System.Xml.XmlDocument();
                _xmlDoc.Load(docFile);
            }
        }
    }

    [Theory]
    [InlineData(MyClass.CtorId)]
    [InlineData(MyClass.CtorWithIntId)]
    [InlineData(MyClass.MessageId)]
    [InlineData(MyClass.PIId)]
    [InlineData(MyClass.FuncId)]
    [InlineData(MyClass.SomeMethodId)]
    [InlineData(MyClass.AnotherMethodId)]
    [InlineData(MyClass.OperatorId)]
    [InlineData(MyClass.PropId)]
    [InlineData(MyClass.OnHappenedId)]
    [InlineData(MyClass.ItemId)]
    [InlineData(MyClass.ExplicitOperatorId)]
    [InlineData(MyClass.GenericMethodId)]
    [InlineData(MyClass.GenericMethodWithParamsId)]
    [InlineData(MyClass.TypeId)]
    [InlineData(MyClass.NestedId)]
    [InlineData(MyClass.DelId)]
    [InlineData(MyClass.TakesClosedGenericId)]
    [InlineData(MyClass.TakeOpenGenericId)]
    [InlineData(MyGenericClass<int>.TypeId)]
    [InlineData(MyGenericClass<int>.CtorId)]
    [InlineData(MyGenericClass<int>.MessageId)]
    [InlineData(MyGenericClass<int>.FuncId)]
    [InlineData(MyGenericClass<int>.SomeMethodId)]
    [InlineData(MyGenericClass<int>.AnotherMethodId)]
    public void DocId_Exists_InDocFile(string docId)
    {
        Assert.True(DocFileContains(docId));

        static bool DocFileContains(string docName)
        {
            if (_xmlDoc is null)
            {
                throw new System.IO.FileNotFoundException("XML documentation file not found.");
            }

            System.Xml.XmlNodeList? memberNodes = _xmlDoc.SelectNodes("//member");
            if (memberNodes is null)
            {
                throw new System.InvalidOperationException("No member nodes found in XML documentation file.");
            }

            foreach (System.Xml.XmlNode memberNode in memberNodes)
            {
                if (memberNode.Attributes?["name"]?.Value == docName)
                {
                    return true;
                }
            }

            return false;
        }
    }

    [Theory]
    [InlineData(typeof(MyClass), MyClass.TypeId)]
    [InlineData(typeof(MyClass.Nested), MyClass.NestedId)]
    [InlineData(typeof(MyClass.Del), MyClass.DelId)]
    [InlineData(typeof(MyGenericClass<>), MyGenericClass<int>.TypeId)]
    public void GetDocId_ShouldReturnExpectedName_ForType(Type type, string expected)
    {
        string actual = type.GetDocId();
        _output.WriteLine($"[X] {expected}");
        _output.WriteLine($"[A] {actual}");
        Assert.Equal(expected, actual);
    }

    [Theory]
    [InlineData(typeof(MyClass), 0, MyClass.CtorId)]
    [InlineData(typeof(MyClass), 1, MyClass.CtorWithIntId)]
    [InlineData(typeof(MyGenericClass<>), 0, MyGenericClass<int>.CtorId)]
    public void GetDocId_ShouldReturnExpectedName_ForCtor(Type type, int ctorIndex, string expected)
    {
        ConstructorInfo ctor = type.GetConstructors()[ctorIndex];
        string actual = ctor.GetDocId();
        _output.WriteLine($"[X] {expected}");
        _output.WriteLine($"[A] {actual}");
        Assert.Equal(expected, actual);
    }

    [Theory]
    [MemberData(nameof(GetDocId_ShouldReturnExpectedName_Data))]
    public void GetDocId_ShouldReturnExpectedName_ForMember(Type type, string memberName, string expected)
    {
        MemberInfo[] members = type.GetMember(memberName);
        MemberInfo member = members.Single(); string actual = member.GetDocId();
        _output.WriteLine($"[X] {expected}");
        _output.WriteLine($"[A] {actual}");
        Assert.Equal(expected, actual);
    }

    [Fact]
    public void GetDocId_ShouldReturnCorrectDocId_ForGenericType()
    {
        // Arrange
        var memberInfo = typeof(TestClass<>).GetMethod(nameof(TestClass<object>.GenericMethod));

        // Act
        var docId = memberInfo!.GetDocId();

        // Assert
        Assert.Equal("M:Tests.MemberInfoExtensionsTests.GetDocId.TestClass`1.GenericMethod``1(System.String)", docId);
    }

    public static TheoryData<Type, string, string> GetDocId_ShouldReturnExpectedName_Data()
    {
        return new TheoryData<Type, string, string>
        {
            {typeof(MyClass), nameof(MyClass.Message), MyClass.MessageId },
            {typeof(MyClass), nameof(MyClass.PI), MyClass.PIId },
            {typeof(MyClass), nameof(MyClass.Func), MyClass.FuncId },
            {typeof(MyClass), nameof(MyClass.SomeMethod), MyClass.SomeMethodId },
            {typeof(MyClass), nameof(MyClass.AnotherMethod), MyClass.AnotherMethodId },
            {typeof(MyClass), "op_Addition", MyClass.OperatorId },
            {typeof(MyClass), nameof(MyClass.Prop), MyClass.PropId },
            {typeof(MyClass), nameof(MyClass.OnHappened), MyClass.OnHappenedId },
            {typeof(MyClass), "Item", MyClass.ItemId },
            {typeof(MyClass), "op_Explicit", MyClass.ExplicitOperatorId },
            {typeof(MyClass), nameof(MyClass.GenericMethod), MyClass.GenericMethodId },
            {typeof(MyClass), nameof(MyClass.GenericMethodWithParams), MyClass.GenericMethodWithParamsId },
            {typeof(MyClass), nameof(MyClass.TakesClosedGeneric), MyClass.TakesClosedGenericId },
            {typeof(MyClass), nameof(MyClass.TakeOpenGeneric), MyClass.TakeOpenGenericId },
            {typeof(MyGenericClass<>), nameof(MyGenericClass<int>.Message), MyGenericClass<int>.MessageId },
            {typeof(MyGenericClass<>), nameof(MyGenericClass<int>.Func), MyGenericClass<int>.FuncId },
            {typeof(MyGenericClass<>), nameof(MyGenericClass<int>.SomeMethod), MyGenericClass<int>.SomeMethodId },
            {typeof(MyGenericClass<>), nameof(MyGenericClass<int>.AnotherMethod), MyGenericClass<int>.AnotherMethodId },
        };
    }

    internal class TestClass<T>
    {
        public void GenericMethod<U>(string param) { }
    }
}