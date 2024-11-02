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
}

