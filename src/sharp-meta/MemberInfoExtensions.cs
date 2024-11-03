using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;

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
    /// <param name="attributeDatas">The list of <see cref="CustomAttributeData"/> instances to search.</param>
    /// <param name="attributeData">When this method returns, contains the <see cref="CustomAttributeData"/> for the specified attribute type, if found; otherwise, <see langword="null"/>.</param>
    /// <returns><see langword="true"/> if the <see cref="CustomAttributeData"/> for the specified attribute type is found; otherwise, <see langword="false"/>.</returns>
    public static bool TryGetCustomAttributeData<T>(this IList<CustomAttributeData>? attributeDatas, [NotNullWhen(true)] out CustomAttributeData? attributeData)
        where T : Attribute
    {
        if (attributeDatas is null)
        {
            attributeData = null;
            return false;
        }

        foreach (CustomAttributeData cad in attributeDatas)
        {
            if (cad.AttributeType.Name == typeof(T).Name)
            {
                attributeData = cad;
                return true;
            }
        }

        attributeData = null;
        return false;
    }

    /// <summary>
    /// Tries to get the <see cref="CustomAttributeData"/> for the specified <see cref="MemberInfo"/> and attribute type.
    /// </summary>
    /// <typeparam name="T">The type of the attribute.</typeparam>
    /// <param name="memberInfo">The <see cref="MemberInfo"/> to check.</param>
    /// <param name="attributeData">When this method returns, contains the <see cref="CustomAttributeData"/> for the specified attribute type, if found; otherwise, <see langword="null"/>.</param>
    /// <returns><see langword="true"/> if the <see cref="CustomAttributeData"/> for the specified attribute type is found; otherwise, <see langword="false"/>.</returns>
    public static bool TryGetCustomAttributeData<T>(this MemberInfo memberInfo, [NotNullWhen(true)] out CustomAttributeData? attributeData)
        where T : Attribute
    {
        return memberInfo.TryGetCustomAttributeData(typeof(T), out attributeData);
    }

    /// <summary>
    /// Tries to get the <see cref="CustomAttributeData"/> for the specified <see cref="MemberInfo"/> and attribute type.
    /// </summary>
    /// <param name="memberInfo">The <see cref="MemberInfo"/> to check.</param>
    /// <param name="attributeType">The attribute type.</param>
    /// <param name="attributeData">When this method returns, contains the <see cref="CustomAttributeData"/> for the specified attribute type, if found; otherwise, <see langword="null"/>.</param>
    /// <returns><see langword="true"/> if the <see cref="CustomAttributeData"/> for the specified attribute type is found; otherwise, <see langword="false"/>.</returns>
    public static bool TryGetCustomAttributeData(this MemberInfo memberInfo, Type attributeType, [NotNullWhen(true)] out CustomAttributeData? attributeData)
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
    public static bool TryGetCustomAttributeData(this MemberInfo memberInfo, string attributeFullName, [NotNullWhen(true)] out CustomAttributeData? attributeData)
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

        //// -- local functions --

        static bool TryGetCustomAttributesDataFromTypeMember(Type type, string memberName, string attributeFullName, [NotNullWhen(true)] out CustomAttributeData? attributeData)
        {
            MemberInfo[] membersInfos = type.GetMember(memberName);
            foreach (MemberInfo member in membersInfos)
            {
                foreach (CustomAttributeData cad in member.GetCustomAttributesData())
                {
                    if (cad.AttributeType.FullName == attributeFullName)
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
    public static IList<CustomAttributeData>? GetAllCustomAttributeData(this MemberInfo memberInfo, bool includeInherited)
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
        // declaring type, so see if the attribute is declared on a member of the same name.
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

        //// -- local functions --

        static List<CustomAttributeData> GetCustomAttributesDataFromTypeMember(Type type, string memberName)
        {
            List<CustomAttributeData> attributeData = [];

            MemberInfo[] membersInfos = type.GetMember(memberName);
            foreach (MemberInfo member in membersInfos)
            {
                attributeData.AddRange(member.GetCustomAttributesData());
            }

            return attributeData;
        }
    }
}

