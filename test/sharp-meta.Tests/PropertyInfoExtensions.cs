using SharpMeta;
using Xunit;

namespace Tests.PropertyInfoExtensions;

public class IsNullable
{
    [Fact]
    public void ShouldReturnTrue_ForNullableValueType()
    {
        System.Reflection.PropertyInfo? property = typeof(TestClass).GetProperty(nameof(TestClass.NullableValueType));
        bool result = property!.IsNullable();
        Assert.True(result);
    }

    [Fact]
    public void ShouldReturnFalse_ForNonNullableValueType()
    {
        System.Reflection.PropertyInfo? property = typeof(TestClass).GetProperty(nameof(TestClass.NonNullableValueType));
        bool result = property!.IsNullable();
        Assert.False(result);
    }

    [Fact]
    public void ShouldReturnTrue_ForNullableReferenceType()
    {
        System.Reflection.PropertyInfo? property = typeof(TestClass).GetProperty(nameof(TestClass.NullableReferenceType));
        bool result = property!.IsNullable();
        Assert.True(result);
    }

    [Fact]
    public void ShouldReturnFalse_ForNonNullableReferenceType()
    {
        System.Reflection.PropertyInfo? property = typeof(TestClass).GetProperty(nameof(TestClass.NonNullableReferenceType));
        bool result = property!.IsNullable();
        Assert.False(result);
    }
}

public class IsNullableReference
{
    [Fact]
    public void ShouldReturnTrue_ForNullableReferenceType()
    {
        System.Reflection.PropertyInfo? property = typeof(TestClass).GetProperty(nameof(TestClass.NullableReferenceType));
        bool result = property!.IsNullableReference();
        Assert.True(result);
    }

    [Fact]
    public void ShouldReturnFalse_ForNonNullableReferenceType()
    {
        System.Reflection.PropertyInfo? property = typeof(TestClass).GetProperty(nameof(TestClass.NonNullableReferenceType));
        bool result = property!.IsNullableReference();
        Assert.False(result);
    }

    [Fact]
    public async Task ShouldBeThreadSafe()
    {
        System.Reflection.PropertyInfo? property = typeof(TestClass).GetProperty(nameof(TestClass.NullableReferenceType));
        var tasks = new List<Task<bool>>();

        for (int i = 0; i < 100; i++)
        {
            tasks.Add(Task.Run(() => property!.IsNullableReference()));
        }

        await Task.WhenAll(tasks);

        foreach (Task<bool> task in tasks)
        {
            Assert.True(await task);
        }
    }
}

public class IsRequiredMember
{
    [Fact]
    public void ShouldReturnTrue_ForRequiredMember()
    {
        System.Reflection.PropertyInfo? property = typeof(RequiredMemberTestClass).GetProperty(nameof(RequiredMemberTestClass.RequiredProperty));
        bool result = property!.IsRequiredMember();
        Assert.True(result);
    }

    [Fact]
    public void ShouldReturnFalse_ForNonRequiredMember()
    {
        System.Reflection.PropertyInfo? property = typeof(RequiredMemberTestClass).GetProperty(nameof(RequiredMemberTestClass.NonRequiredProperty));
        bool result = property!.IsRequiredMember();
        Assert.False(result);
    }
}

internal class TestClass
{
    public int NonNullableValueType { get; set; }

    public int? NullableValueType { get; set; }

    public string NonNullableReferenceType { get; set; } = string.Empty;

    public string? NullableReferenceType { get; set; }
}

internal class RequiredMemberTestClass
{
    public required string RequiredProperty { get; set; } = string.Empty;

    public string NonRequiredProperty { get; set; } = string.Empty;
}