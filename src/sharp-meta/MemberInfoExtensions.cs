using System.Collections.Concurrent;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using System.Security;
using System.Xml.Linq;

namespace SharpMeta;

/// <summary>
/// Provides extension methods for <see cref="MemberInfo"/> instances.
/// </summary>
public static class MemberInfoExtensions
{
    // Add a private static dictionary to cache the XDocument objects for each assembly
    private static readonly ConcurrentDictionary<string, XDocument?> XmlDocumentationCache = new();

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

    /// <summary>
    /// Tries to get the XML documentation comments for the specified <see cref="MemberInfo"/>.
    /// </summary>
    /// <param name="memberInfo">The <see cref="MemberInfo"/> to get the documentation comments for.</param>
    /// <param name="documentationComments">When this method returns, contains the XML documentation comments for the specified <see cref="MemberInfo"/>, if found; otherwise, <see langword="null"/>.</param>
    /// <returns><see langword="true"/> if the XML documentation comments for the specified <see cref="MemberInfo"/> are found; otherwise, <see langword="false"/>.</returns>
    public static bool TryGetDocComments(this MemberInfo memberInfo, [NotNullWhen(true)] out DocComments? documentationComments)
    {
        documentationComments = null;
        if (memberInfo is null)
            return false;

        // Get the XML documentation file path
        string xmlDocumentationPath = Path.ChangeExtension(memberInfo.Module.Assembly.Location, ".xml");

        // Check the cache for the XDocument or load it from the file
        XDocument? xmlDoc = XmlDocumentationCache.GetOrAdd(xmlDocumentationPath, path =>
        {
            try
            {
                return XDocument.Load(path);
            }
            catch (Exception ex) when (ex is SecurityException or FileNotFoundException or ArgumentNullException)
            {
                return null;
            }
        });

        if (xmlDoc is null)
            return false;

        // Create the member name in the format used in the XML documentation file
        string memberName = memberInfo is Type type
            ? $"T:{type.FullName}"
            : $"{memberInfo.MemberType.ToString()[0]}:{memberInfo.DeclaringType?.FullName}.{memberInfo.Name}";

        // Find the documentation element for the member
        XElement? memberElement = xmlDoc.Descendants("member")
            .FirstOrDefault(e => e.Attribute("name")?.Value == memberName);

        if (memberElement is not null)
        {
            string? summary = memberElement.Element("summary")?.Value.Trim();
            string? returns = memberElement.Element("returns")?.Value.Trim();

            documentationComments = new DocComments(
                string.IsNullOrEmpty(summary) ? null : summary,
                [.. memberElement.Elements("param").Select(e => (e.Attribute("name")?.Value ?? string.Empty, e.Value.Trim()))],
                string.IsNullOrEmpty(returns) ? null : returns);

            return true;
        }

        return false;
    }
}
