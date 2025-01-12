using System.Collections.Concurrent;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using System.Security;
using System.Text;
using System.Xml.Linq;

namespace SharpMeta;

/// <summary>
/// Provides extension methods for <see cref="MemberInfo"/> instances.
/// </summary>
public static class MemberInfoExtensions
{
    /// <summary>
    /// Tries to get the <see cref="CustomAttributeData"/> from the attribute data collection and attribute type.
    /// </summary>
    /// <typeparam name="T">The type of the attribute.</typeparam>
    /// <param name="attributes">The list of <see cref="CustomAttributeData"/> instances to search.</param>
    /// <param name="attributeData">When this method returns, contains the <see cref="CustomAttributeData"/> for the specified attribute type, if found; otherwise, <see langword="null"/>.</param>
    /// <returns><see langword="true"/> if the <see cref="CustomAttributeData"/> for the specified attribute type is found; otherwise, <see langword="false"/>.</returns>
    public static bool TryGetCustomAttributeData<T>(
        this IList<CustomAttributeData>? attributes,
        [NotNullWhen(true)] out CustomAttributeData? attributeData)
        where T : Attribute
    {
        attributeData = null;

        if (attributes is null)
            return false;

        string? targetName = typeof(T).FullName;

        if (targetName is null)
            return false;

        foreach (CustomAttributeData cad in attributes)
        {
            string? attributeName = cad.AttributeType.FullName;

            if (attributeName is null)
                continue;

            if (attributeName.Equals(targetName, StringComparison.OrdinalIgnoreCase))
            {
                attributeData = cad;
                return true;
            }
        }

        return false;
    }

    /// <summary>
    /// Gets the <see cref="CustomAttributeData"/> for the specified <see cref="MemberInfo"/> and attribute type.
    /// </summary>
    /// <typeparam name="T">The type of the attribute.</typeparam>
    /// <param name="memberInfo">The <see cref="MemberInfo"/> to get the custom attribute data for.</param>
    /// <returns>The <see cref="CustomAttributeData"/> for the specified attribute type, if found; otherwise, <see langword="null"/>.</returns>
    public static CustomAttributeData? GetCustomAttributeData<T>(this MemberInfo memberInfo)
        where T : Attribute
    {
        return memberInfo.TryGetCustomAttributeData<T>(out CustomAttributeData? attributeData)
            ? attributeData
            : null;
    }

    /// <summary>
    /// Tries to get the <see cref="CustomAttributeData"/> for the specified <see cref="MemberInfo"/> and attribute type.
    /// </summary>
    /// <typeparam name="T">The type of the attribute.</typeparam>
    /// <param name="memberInfo">The <see cref="MemberInfo"/> to check.</param>
    /// <param name="attributeData">When this method returns, contains the <see cref="CustomAttributeData"/> for the specified attribute type, if found; otherwise, <see langword="null"/>.</param>
    /// <returns><see langword="true"/> if the <see cref="CustomAttributeData"/> for the specified attribute type is found; otherwise, <see langword="false"/>.</returns>
    public static bool TryGetCustomAttributeData<T>(
        this MemberInfo memberInfo,
        [NotNullWhen(true)] out CustomAttributeData? attributeData)
        where T : Attribute
    {
        return memberInfo.TryGetCustomAttributeData(typeof(T), out attributeData);
    }

    /// <summary>
    /// Gets the <see cref="CustomAttributeData"/> for the specified <see cref="MemberInfo"/> and attribute type.
    /// </summary>
    /// <param name="memberInfo">The <see cref="MemberInfo"/> to get the custom attribute data for.</param>
    /// <param name="attributeType">The type of the attribute to get the custom attribute data for.</param>
    /// <returns>The <see cref="CustomAttributeData"/> for the specified attribute type, if found; otherwise, <see langword="null"/>.</returns>
    public static CustomAttributeData? GetCustomAttributeData(this MemberInfo memberInfo, Type attributeType)
    {
        return memberInfo.TryGetCustomAttributeData(attributeType, out CustomAttributeData? attributeData)
            ? attributeData
            : null;
    }

    /// <summary>
    /// Tries to get the <see cref="CustomAttributeData"/> for the specified <see cref="MemberInfo"/> and attribute type.
    /// </summary>
    /// <param name="memberInfo">The <see cref="MemberInfo"/> to check.</param>
    /// <param name="attributeType">The attribute type.</param>
    /// <param name="attributeData">When this method returns, contains the <see cref="CustomAttributeData"/> for the specified attribute type, if found; otherwise, <see langword="null"/>.</param>
    /// <returns><see langword="true"/> if the <see cref="CustomAttributeData"/> for the specified attribute type is found; otherwise, <see langword="false"/>.</returns>
    public static bool TryGetCustomAttributeData(
        this MemberInfo memberInfo,
        Type attributeType,
        [NotNullWhen(true)] out CustomAttributeData? attributeData)
    {
        ArgumentNullException.ThrowIfNull(memberInfo);
        ArgumentNullException.ThrowIfNull(attributeType);

        return memberInfo.TryGetCustomAttributeData(
            attributeType.FullName ?? throw new InvalidOperationException("Attribute type has no full name."),
            out attributeData);
    }

    /// <summary>
    /// Tries to get the <see cref="CustomAttributeData"/> for the specified <see cref="Type"/> and attribute type.
    /// </summary>
    /// <param name="memberInfo">The <see cref="MemberInfo"/> to check.</param>
    /// <param name="attributeFullName">The full name of the attribute type.</param>
    /// <param name="attributeData">When this method returns, contains the <see cref="CustomAttributeData"/> for the specified attribute type, if found; otherwise, <see langword="null"/>.</param>
    /// <returns><see langword="true"/> if the <see cref="CustomAttributeData"/> for the specified attribute type is found; otherwise, <see langword="false"/>.</returns>
    public static bool TryGetCustomAttributeData(
        this MemberInfo memberInfo,
        string attributeFullName,
        [NotNullWhen(true)] out CustomAttributeData? attributeData)
    {
        ArgumentNullException.ThrowIfNull(memberInfo);
        ArgumentException.ThrowIfNullOrWhiteSpace(attributeFullName);

        attributeData = null;

        foreach (CustomAttributeData cad in memberInfo.GetCustomAttributesData())
        {
            if (cad.AttributeType.FullName == attributeFullName)
            {
                attributeData = cad;
                return true;
            }
        }

        Type? baseType = memberInfo.DeclaringType;
        while (baseType is not null)
        {
            foreach (Type type in baseType.GetInterfaces())
            {
                if (TryGetCustomAttributesDataFromTypeMember(type, memberInfo.Name, attributeFullName, out attributeData))
                {
                    return true;
                }
            }

            if (TryGetCustomAttributesDataFromTypeMember(baseType, memberInfo.Name, attributeFullName, out attributeData))
            {
                return true;
            }

            baseType = baseType.BaseType;
        }

        return attributeData is not null;

        static bool TryGetCustomAttributesDataFromTypeMember(
            Type type,
            string memberName,
            string attributeFullName,
            [NotNullWhen(true)] out CustomAttributeData? attributeData)
        {
            MemberInfo[] membersInfos = type.GetMember(memberName);
            foreach (MemberInfo member in membersInfos)
            {
                foreach (CustomAttributeData cad in member.GetCustomAttributesData())
                {
                    string? cadName = cad.AttributeType.FullName;

                    if (cadName is null)
                        continue;

                    if (cadName.Equals(attributeFullName, StringComparison.OrdinalIgnoreCase))
                    {
                        attributeData = cad;
                        return true;
                    }
                }
            }

            attributeData = null;
            return false;
        }
    }

    /// <summary>
    /// Gets the custom attribute data for the specified <see cref="MemberInfo"/>.
    /// </summary>
    /// <param name="memberInfo">The <see cref="MemberInfo"/> to get the custom attribute data for.</param>
    /// <param name="includeInherited">Specifies whether to include custom attribute data from inherited members.</param>
    /// <returns>The custom attribute data for the specified <see cref="MemberInfo"/>.</returns>
    public static IList<CustomAttributeData>? GetAllCustomAttributeData(
        this MemberInfo memberInfo,
        bool includeInherited)
    {
        ArgumentNullException.ThrowIfNull(memberInfo);

        ImmutableList<CustomAttributeData>.Builder attributeData = ImmutableList.CreateBuilder<CustomAttributeData>();
        attributeData.AddRange(memberInfo.GetCustomAttributesData());

        if (!includeInherited)
        {
            return attributeData.ToImmutable();
        }

        Type? baseType;

        // If the MemberInfo is for a Type, iterate through the base types for the attribute.
        if (memberInfo is Type typeInfo)
        {
            baseType = typeInfo.BaseType;
            while (baseType is not null)
            {
                attributeData.AddRange(baseType.GetCustomAttributesData());
                baseType = baseType.BaseType;
            }

            return attributeData.ToImmutable();
        }

        // If the MemberInfo is for a PropertyInfo, or FieldInfo, iterate through the base types of the
        // declaring type, to see if the attribute is declared on a member of the same name.
        baseType = memberInfo switch
        {
            PropertyInfo pi => pi.DeclaringType,
            FieldInfo fi => fi.DeclaringType,
            _ => null,
        };

        while (baseType is not null)
        {
            foreach (Type type in baseType.GetInterfaces())
            {
                attributeData.AddRange(GetCustomAttributesDataFromTypeMember(type, memberInfo.Name));
            }

            attributeData.AddRange(GetCustomAttributesDataFromTypeMember(baseType, memberInfo.Name));

            baseType = baseType.BaseType;
        }

        return attributeData.ToImmutable();

        static IEnumerable<CustomAttributeData> GetCustomAttributesDataFromTypeMember(Type type, string memberName)
        {
            MemberInfo[] membersInfos = type.GetMember(memberName);
            foreach (MemberInfo member in membersInfos)
            {
                foreach (CustomAttributeData cad in member.GetCustomAttributesData())
                {
                    yield return cad;
                }
            }
        }
    }

    /// <summary>
    /// Gets the documentation comments for the specified <see cref="MemberInfo"/>.
    /// </summary>
    /// <param name="memberInfo">The <see cref="MemberInfo"/> to get the documentation comments for.</param>
    /// <returns>A <see cref="DocComments"/> instance containing the documentation comments, if found; otherwise, <see langword="null"/>.</returns>
    public static DocComments? GetDocComments(this MemberInfo memberInfo)
    {
        return DocComments.Parse(memberInfo);
    }

    /// <summary>
    /// Tries to get the documentation comments for the specified <see cref="MemberInfo"/>.
    /// </summary>
    /// <param name="memberInfo">The <see cref="MemberInfo"/> to get the documentation comments for.</param>
    /// <param name="docComments">When this method returns, contains the <see cref="DocComments"/> for the specified <see cref="MemberInfo"/>, if found; otherwise, <see langword="null"/>.</param>
    /// <returns><see langword="true"/> if the documentation comments for the specified <see cref="MemberInfo"/> are found; otherwise, <see langword="false"/>.</returns>
    public static bool TryGetDocComments(this MemberInfo memberInfo, [NotNullWhen(true)] out DocComments? docComments)
    {
        docComments = memberInfo.GetDocComments();
        return docComments is not null;
    }

    /// <summary>
    /// Gets the documentation ID for the specified <see cref="MemberInfo"/>.
    /// </summary>
    /// <param name="memberInfo">The <see cref="MemberInfo"/> to get the documentation name for.</param>
    /// <returns>The documentation ID for the specified <see cref="MemberInfo"/> per <see href="https://learn.microsoft.com/en-us/dotnet/csharp/language-reference/xmldoc/#id-strings"/>.</returns>
    /// <exception cref="NotSupportedException">Thrown when the member type is not supported.</exception>
    /// <seealso href="https://learn.microsoft.com/en-us/dotnet/csharp/language-reference/xmldoc/#id-strings"/>
    public static string GetDocId(this MemberInfo memberInfo)
    {
        ArgumentNullException.ThrowIfNull(memberInfo);

        if (memberInfo.DeclaringType is null)
        {
            return memberInfo switch
            {
                Type type => $"T:{TypeNameInfo.From(type).GetFullName(NameStyle.DocId)}",
                _ => throw new NotSupportedException($"Member type with null declaring type not supported: {memberInfo.GetType().FullName}")
            };
        }

        TypeNameInfo declaringTypeName = memberInfo.DeclaringType;

        return memberInfo switch
        {
            MethodInfo method => GetMethodDocId(method, declaringTypeName),
            ConstructorInfo ctor => $"M:{declaringTypeName.GetFullName(NameStyle.DocId)}.#ctor{GetMethodParameters(ctor)}",
            PropertyInfo property when property.GetIndexParameters().Length > 0 => throw new NotSupportedException("Indexer properties are not supported."),
            PropertyInfo property => GetPropertyDocId(property, declaringTypeName),
            FieldInfo field => $"F:{declaringTypeName.GetFullName(NameStyle.DocId)}.{ConformItemName(field.Name)}",
            EventInfo @event => $"E:{declaringTypeName.GetFullName(NameStyle.DocId)}.{ConformItemName(@event.Name)}",
            Type type when typeof(Delegate).IsAssignableFrom(type) => $"T:{declaringTypeName.GetFullName(NameStyle.DocId)}.{ConformItemName(type.Name)}",
            Type type => $"T:{TypeNameInfo.From(type).GetFullName(NameStyle.DocId)}",
            _ => throw new NotSupportedException($"Member type not supported: {memberInfo.GetType().FullName}")
        };

        static string GetMethodDocId(MethodInfo method, TypeNameInfo declaringTypeName)
        {
            StringBuilder sb = new StringBuilder("M:")
                .Append(declaringTypeName.GetFullName(NameStyle.DocId))
                .Append('.')
                .Append(ConformItemName(method.Name));

            if (method.IsGenericMethod)
            {
                sb.Append("``").Append(method.GetGenericArguments().Length);
            }

            sb.Append(GetMethodParameters(method));
            if (method.Name is "op_Implicit" or "op_Explicit")
            {
                sb.Append('~').Append(TypeNameInfo.From(method.ReturnType).GetFullName(NameStyle.DocIdParameter));
            }

            return sb.ToString();
        }

        static string GetPropertyDocId(PropertyInfo property, TypeNameInfo declaringTypeName)
        {
            StringBuilder sb = new StringBuilder("P:")
                .Append(declaringTypeName.GetFullName(NameStyle.DocId))
                .Append('.')
                .Append(ConformItemName(property.Name));

            return sb.ToString();
        }

        static string ConformItemName(string name)
        {
            return name.Replace('.', '#');
        }

        static string GetMethodParameters(MethodBase method)
        {
            ParameterInfo[] parameters = method.GetParameters();
            if (parameters.Length == 0)
                return string.Empty;

            return $"({string.Join(",", parameters.Select(p => TypeNameInfo.From(p.ParameterType).GetFullName(NameStyle.DocIdParameter)))})";
        }
    }
}

