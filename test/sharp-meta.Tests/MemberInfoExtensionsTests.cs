using System.Reflection;
using SharpMeta;
using Tests.Data;
using Xunit;

namespace Tests;

public partial class MemberInfoExtensionsTests
{
    public class GetAllCustomAttributeData
    {
        [Fact]
        public void ShouldReturnAllAttributes()
        {
            Type type = typeof(SampleClass);
            IList<CustomAttributeData>? attributes = type.GetAllCustomAttributeData(includeInherited: false);
            Assert.NotNull(attributes);
            Assert.Contains(attributes, a => a.AttributeType == typeof(SampleAttribute));
        }

        [Fact]
        public void ShouldIncludeInheritedAttributes()
        {
            Type type = typeof(DerivedSampleClass);
            IList<CustomAttributeData>? attributes = type.GetAllCustomAttributeData(includeInherited: true);
            Assert.NotNull(attributes);
            Assert.Contains(attributes, a => a.AttributeType == typeof(SampleAttribute));
        }

        [Fact]
        public void ShouldReturnAttributesForMember()
        {
            MethodInfo? member = typeof(SampleClass).GetMethod(nameof(SampleClass.SampleMethod));
            IList<CustomAttributeData>? attributes = member!.GetAllCustomAttributeData(includeInherited: false);
            Assert.NotNull(attributes);
            Assert.Contains(attributes, a => a.AttributeType == typeof(SampleAttribute));
        }

        [Fact]
        public void ShouldIncludeInheritedAttributesForMember()
        {
            MethodInfo? member = typeof(DerivedSampleClass).GetMethod(nameof(DerivedSampleClass.SampleMethod));
            IList<CustomAttributeData>? attributes = member!.GetAllCustomAttributeData(includeInherited: true);
            Assert.NotNull(attributes);
            Assert.Contains(attributes, a => a.AttributeType == typeof(SampleAttribute));
        }

        [Fact]
        public void ShouldReturnAttributesForProperty()
        {
            PropertyInfo? property = typeof(SampleClass).GetProperty(nameof(SampleClass.AttributedProperty));
            IList<CustomAttributeData>? attributes = property!.GetAllCustomAttributeData(includeInherited: false);
            Assert.NotNull(attributes);
            Assert.Contains(attributes, a => a.AttributeType == typeof(SampleAttribute));
        }

        [Fact]
        public void ShouldIncludeInheritedAttributesForProperty()
        {
            PropertyInfo? property = typeof(DerivedSampleClass).GetProperty(nameof(DerivedSampleClass.AttributedProperty));
            IList<CustomAttributeData>? attributes = property!.GetAllCustomAttributeData(includeInherited: true);
            Assert.NotNull(attributes);
            Assert.Contains(attributes, a => a.AttributeType == typeof(SampleAttribute));
        }
    }

    public class GetCustomAttributeData
    {
        [Fact]
        public void FromMemberInfo_NotNull()
        {
            MethodInfo? member = typeof(SampleClass).GetMethod(nameof(SampleClass.SampleMethod));
            CustomAttributeData? result = member!.GetCustomAttributeData<SampleAttribute>();
            Assert.NotNull(result);
        }

        [Fact]
        public void FromMemberInfoAndType_NotNull()
        {
            MethodInfo? member = typeof(SampleClass).GetMethod(nameof(SampleClass.SampleMethod));
            CustomAttributeData? result = member!.GetCustomAttributeData(typeof(SampleAttribute));
            Assert.NotNull(result);
        }
    }

    public class TryGetCustomAttributeData
    {
        [Fact]
        public void FromAttributeDataCollection_ShouldReturnTrue()
        {
            Type type = typeof(SampleClass);
            CustomAttributeData attributeData = type.GetCustomAttributesData().First(a => a.AttributeType == typeof(SampleAttribute));
            var attributeDataList = new List<CustomAttributeData> { attributeData };
            bool result = attributeDataList.TryGetCustomAttributeData<SampleAttribute>(out CustomAttributeData? foundAttributeData);
            Assert.True(result);
            Assert.NotNull(foundAttributeData);
        }

        [Fact]
        public void FromMemberInfo_ShouldReturnTrue()
        {
            MethodInfo? member = typeof(SampleClass).GetMethod(nameof(SampleClass.SampleMethod));
            bool result = member!.TryGetCustomAttributeData<SampleAttribute>(out CustomAttributeData? attributeData);
            Assert.True(result);
            Assert.NotNull(attributeData);
        }

        [Fact]
        public void FromMemberInfoAndType_ShouldReturnTrue()
        {
            MethodInfo? member = typeof(SampleClass).GetMethod(nameof(SampleClass.SampleMethod));
            bool result = member!.TryGetCustomAttributeData(typeof(SampleAttribute), out CustomAttributeData? attributeData);
            Assert.True(result);
            Assert.NotNull(attributeData);
        }

        [Fact]
        public void FromMemberInfoAndFullName_ShouldReturnTrue()
        {
            MethodInfo? member = typeof(SampleClass).GetMethod(nameof(SampleClass.SampleMethod));
            bool result = member!.TryGetCustomAttributeData("Tests.Data.SampleAttribute", out CustomAttributeData? attributeData);
            Assert.True(result);
            Assert.NotNull(attributeData);
        }
    }

    public class TryGetDocComments
    {
        [Fact]
        public void ShouldReturnTrueWhenDocCommentsExist()
        {
            MethodInfo? member = typeof(SampleClass).GetMethod(nameof(SampleClass.SampleMethod));
            bool result = member!.TryGetDocComments(out DocComments? docComments);
            Assert.True(result);
            Assert.NotNull(docComments);
        }

        [Fact]
        public void ShouldReturnFalseWhenDocCommentsDoNotExist()
        {
            MethodInfo? member = typeof(SampleClass).GetMethod(nameof(SampleClass.MethodWithoutDocComments));
            bool result = member!.TryGetDocComments(out DocComments? docComments);
            Assert.False(result);
            Assert.Null(docComments);
        }
    }
}
