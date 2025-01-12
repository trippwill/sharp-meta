namespace Tests.Data;

/// <summary>
/// Enter description here for class X.
/// ID string generated is "T:Tests.Data.MyClass".
/// </summary>
public unsafe class MyClass
{
    public const string TypeId = "T:Tests.Data.MyClass";
    public const string CtorId = "M:Tests.Data.MyClass.#ctor";
    public const string CtorWithIntId = "M:Tests.Data.MyClass.#ctor(System.Int32)";
    public const string MessageId = "F:Tests.Data.MyClass.Message";
    public const string PIId = "F:Tests.Data.MyClass.PI";
    public const string FuncId = "M:Tests.Data.MyClass.Func";
    public const string SomeMethodId = "M:Tests.Data.MyClass.SomeMethod(System.String,System.Int32@,System.Void*)";
    public const string AnotherMethodId = "M:Tests.Data.MyClass.AnotherMethod(System.Int16[],System.Int32[0:,0:])";
    public const string OperatorId = "M:Tests.Data.MyClass.op_Addition(Tests.Data.MyClass,Tests.Data.MyClass)";
    public const string PropId = "P:Tests.Data.MyClass.Prop";
    public const string OnHappenedId = "E:Tests.Data.MyClass.OnHappened";
    public const string ItemId = "P:Tests.Data.MyClass.Item(System.String)";
    public const string NestedId = "T:Tests.Data.MyClass.Nested";
    public const string DelId = "T:Tests.Data.MyClass.Del";
    public const string ExplicitOperatorId = "M:Tests.Data.MyClass.op_Explicit(Tests.Data.MyClass)~System.Int32";
    public const string GenericMethodId = "M:Tests.Data.MyClass.GenericMethod``1(``0)";
    public const string GenericMethodWithParamsId = "M:Tests.Data.MyClass.GenericMethodWithParams``1(``0,System.String)";
    public const string TakesClosedGenericId = "M:Tests.Data.MyClass.TakesClosedGeneric(System.Collections.Generic.Dictionary{System.String,System.Int32})";
    public const string TakeOpenGenericId = "M:Tests.Data.MyClass.TakeOpenGeneric``1(System.Collections.Generic.Dictionary{``0,System.Int32})";

    /// <summary>
    /// Enter description here for the first constructor.
    /// ID string generated is "M:Tests.Data.MyClass.#ctor".
    /// </summary>
    public MyClass() { }

    /// <summary>
    /// Enter description here for the second constructor.
    /// ID string generated is "M:Tests.Data.MyClass.#ctor(System.Int32)".
    /// </summary>
    /// <param name="i">Describe parameter.</param>
    public MyClass(int i) { }

    /// <summary>
    /// Enter description here for field Message.
    /// ID string generated is "F:Tests.Data.MyClass.Message".
    /// </summary>
    public string? Message;

    /// <summary>
    /// Enter description for constant PI.
    /// ID string generated is "F:Tests.Data.MyClass.PI".
    /// </summary>
    public const double PI = 3.14;

    /// <summary>
    /// Enter description for method Func.
    /// ID string generated is "M:Tests.Data.MyClass.Func".
    /// </summary>
    /// <returns>Describe return value.</returns>
    public int Func() => 1;

    /// <summary>
    /// Enter description for method SomeMethod.
    /// ID string generated is "M:Tests.Data.MyClass.SomeMethod(System.String,System.Int32@,System.Void*)".
    /// </summary>
    /// <param name="str">Describe parameter.</param>
    /// <param name="num">Describe parameter.</param>
    /// <param name="ptr">Describe parameter.</param>
    /// <returns>Describe return value.</returns>
    public int SomeMethod(string str, ref int num, void* ptr) { return 1; }

    /// <summary>
    /// Enter description for method AnotherMethod.
    /// ID string generated is "M:Tests.Data.MyClass.AnotherMethod(System.Int16[],System.Int32[0:,0:])".
    /// </summary>
    /// <param name="array1">Describe parameter.</param>
    /// <param name="array">Describe parameter.</param>
    /// <returns>Describe return value.</returns>
    public int AnotherMethod(short[] array1, int[,] array) { return 0; }

    /// <summary>
    /// Enter description for method GenericReturn.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    public T GenericReturn<T>() => default!;

    /// <summary>
    /// Enter description for method TakesClosedGeneric.
    /// </summary>
    /// <param name="dict"></param>
    public void TakesClosedGeneric(Dictionary<string, int> dict) { }

    /// <summary>
    /// Enter description for method TakeOpenGeneric.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="dict"></param>
    public void TakeOpenGeneric<T>(Dictionary<T, int> dict) where T : notnull { }

    /// <summary>
    /// Enter description for operator.
    /// ID string generated is "M:Tests.Data.MyClass.op_Addition(Tests.Data.MyClass,Tests.Data.MyClass)".
    /// </summary>
    /// <param name="first">Describe parameter.</param>
    /// <param name="second">Describe parameter.</param>
    /// <returns>Describe return value.</returns>
    public static MyClass operator +(MyClass first, MyClass second) { return first; }

    /// <summary>
    /// Enter description for property.
    /// ID string generated is "P:Tests.Data.MyClass.Prop".
    /// </summary>
    public int Prop { get { return 1; } set { } }

    /// <summary>
    /// Enter description for event.
    /// ID string generated is "E:Tests.Data.MyClass.OnHappened".
    /// </summary>
    public event Del? OnHappened;

    /// <summary>
    /// Enter description for index.
    /// ID string generated is "P:Tests.Data.MyClass.Item(System.String)".
    /// </summary>
    /// <param name="s">Describe parameter.</param>
    /// <returns>Describe return value.</returns>
    public int this[string s] => 1;

    /// <summary>
    /// Enter description for class Nested.
    /// ID string generated is "T:Tests.Data.MyClass.Nested".
    /// </summary>
    public class Nested { }

    /// <summary>
    /// Enter description for delegate.
    /// ID string generated is "T:Tests.Data.MyClass.Del".
    /// </summary>
    /// <param name="i">Describe parameter.</param>
    public delegate void Del(int i);

    /// <summary>
    /// Enter description for operator.
    /// ID string generated is "M:Tests.Data.MyClass.op_Explicit(Tests.Data.MyClass)~System.Int32".
    /// </summary>
    /// <param name="myParameter">Describe parameter.</param>
    /// <returns>Describe return value.</returns>
    public static explicit operator int(MyClass myParameter) => 1;

    /// <summary>
    /// Enter description for generic method.
    /// ID string generated is "M:Tests.Data.MyClass.GenericMethod``1(``0)".
    /// </summary>
    /// <typeparam name="T">Describe type parameter.</typeparam>
    /// <param name="param">Describe parameter.</param>
    public void GenericMethod<T>(T param) { }

    /// <summary>
    /// Enter description for generic method with parameters.
    /// ID string generated is "M:Tests.Data.MyClass.GenericMethodWithParams``1(``0,System.String)".
    /// </summary>
    /// <typeparam name="T">Describe type parameter.</typeparam>
    /// <param name="param">Describe parameter.</param>
    /// <param name="str">Describe parameter.</param>
    public void GenericMethodWithParams<T>(T param, string str) { }
}

/// <summary>
/// Enter description here for generic class.
/// ID string generated is "T:Tests.Data.MyGenericClass`1".
/// </summary>
/// <typeparam name="T">Describe type parameter.</typeparam>
public unsafe class MyGenericClass<T>
{
    public const string TypeId = "T:Tests.Data.MyGenericClass`1";
    public const string CtorId = "M:Tests.Data.MyGenericClass`1.#ctor";
    public const string MessageId = "F:Tests.Data.MyGenericClass`1.Message";
    public const string FuncId = "M:Tests.Data.MyGenericClass`1.Func";
    public const string SomeMethodId = "M:Tests.Data.MyGenericClass`1.SomeMethod(System.String,System.Int32@,System.Void*)";
    public const string AnotherMethodId = "M:Tests.Data.MyGenericClass`1.AnotherMethod(System.Int16[],System.Int32[0:,0:])";

    /// <summary>
    /// Enter description here for the constructor.
    /// ID string generated is "M:Tests.Data.MyGenericClass`1.#ctor".
    /// </summary>
    public MyGenericClass() { }

    /// <summary>
    /// Enter description here for field Message.
    /// ID string generated is "F:Tests.Data.MyGenericClass`1.Message".
    /// </summary>
    public string? Message;

    /// <summary>
    /// Enter description for method Func.
    /// ID string generated is "M:Tests.Data.MyGenericClass`1.Func".
    /// </summary>
    /// <returns>Describe return value.</returns>
    public int Func() => 1;

    /// <summary>
    /// Enter description for method SomeMethod.
    /// ID string generated is "M:Tests.Data.MyGenericClass`1.SomeMethod(System.String,System.Int32@,System.Void*)".
    /// </summary>
    /// <param name="str">Describe parameter.</param>
    /// <param name="num">Describe parameter.</param>
    /// <param name="ptr">Describe parameter.</param>
    /// <returns>Describe return value.</returns>
    public int SomeMethod(string str, ref int num, void* ptr) { return 1; }

    /// <summary>
    /// Enter description for method AnotherMethod.
    /// ID string generated is "M:Tests.Data.MyGenericClass`1.AnotherMethod(System.Int16[],System.Int32[0:,0:])".
    /// </summary>
    /// <param name="array1">Describe parameter.</param>
    /// <param name="array">Describe parameter.</param>
    /// <returns>Describe return value.</returns>
    public int AnotherMethod(short[] array1, int[,] array) { return 0; }
}