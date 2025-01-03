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
    /// XDocument by file path cache.
    /// </summary>
    private static readonly ConcurrentDictionary<string, XDocument?> XmlDocumentationCache = new(StringComparer.OrdinalIgnoreCase);

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
    /// <returns>The <see cref="DocComments"/> for the specified <see cref="MemberInfo"/>, if found; otherwise, <see langword="null"/>.</returns>
    public static DocComments? GetDocComments(this MemberInfo memberInfo)
    {
        return memberInfo.TryGetDocComments(out DocComments? docComments)
            ? docComments
            : null;
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
        string memberName = memberInfo.GetDocId();

        // Find the documentation element for the member
        XElement? memberElement = xmlDoc.Descendants("member")
            .FirstOrDefault(e => e.Attribute("name")?.Value == memberName);

        if (memberElement is not null)
        {
            string? summary = memberElement.Element("summary")?.Value.Trim();
            string? returns = memberElement.Element("returns")?.Value.Trim();
            string? remarks = memberElement.Element("remarks")?.Value.Trim();
            string? example = memberElement.Element("example")?.Value.Trim();

            var parameters = memberElement.Elements("param")
                .Select(e => (e.Attribute("name")?.Value ?? string.Empty, e.Value.Trim()))
                .ToImmutableArray();

            var exceptions = memberElement.Elements("exception")
                .Select(e => (e.Attribute("cref")?.Value.Split(':').ElementAtOrDefault(1) ?? string.Empty, e.Value.Trim()))
                .ToImmutableArray();

            var typeParameters = memberElement.Elements("typeparam")
                .Select(e => (e.Attribute("name")?.Value ?? string.Empty, e.Value.Trim()))
                .ToImmutableArray();

            documentationComments = new DocComments(
                string.IsNullOrEmpty(summary) ? null : summary,
                string.IsNullOrEmpty(remarks) ? null : remarks,
                string.IsNullOrEmpty(example) ? null : example,
                string.IsNullOrEmpty(returns) ? null : returns,
                parameters,
                typeParameters,
                exceptions);

            return true;
        }

        return false;

        static string GetFullMemberName(MemberInfo memberInfo)
        {
            if (memberInfo.DeclaringType is null)
                return memberInfo.Name;

            return $"{GetFullMemberName(memberInfo.DeclaringType)}.{memberInfo.Name}";
        }
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
                sb.Append('~').Append(GetParameterTypeName(method.ReturnType));
            }

            return sb.ToString();
        }

        static string GetPropertyDocId(PropertyInfo property, TypeNameInfo declaringTypeName)
        {
            StringBuilder sb = new StringBuilder("P:")
                .Append(declaringTypeName.GetFullName(NameStyle.DocId))
                .Append('.')
                .Append(ConformItemName(property.Name));

            if (property.GetIndexParameters().Length > 0)
            {
                sb.Append('(')
                  .Append(string.Join(",", property.GetIndexParameters()
                    .Select(p => GetParameterTypeName(p.ParameterType))))
                  .Append(')');
            }
            return sb.ToString();
        }

        static string ConformItemName(string name)
        {
            return name.Replace('.', '#');
        }

        static string ConformTypeShortName(Type type)
        {
            string typeName = type.Name;
            if (type.IsGenericType)
            {
                typeName = typeName[..typeName.IndexOf('`')];
                typeName = $"{typeName}`{type.GetGenericArguments().Length}";
            }
            return typeName.Replace('+', '.');
        }

        static string GetMethodParameters(MethodBase method)
        {
            ParameterInfo[] parameters = method.GetParameters();
            if (parameters.Length == 0)
                return string.Empty;

            return $"({string.Join(",", parameters.Select(p => TypeNameInfo.From(p.ParameterType).GetFullName(NameStyle.DocIdParameter)))})";
        }

        static string GetParameterTypeName(Type type)
        {
            if (type.IsByRef)
                return $"{GetParameterTypeName(type.GetElementType()!)}@";

            if (type.IsPointer)
                return $"{GetParameterTypeName(type.GetElementType()!)}*";

            if (type.IsArray)
            {
                if (type.GetArrayRank() == 1)
                {
                    return $"{GetParameterTypeName(type.GetElementType()!)}[]";
                }

                var arrayDimensions = new List<string>();
                for (int i = 0; i < type.GetArrayRank(); i++)
                {
                    arrayDimensions.Add("0:");
                }

                return $"{GetParameterTypeName(type.GetElementType()!)}[{string.Join(",", arrayDimensions)}]";
            }

            if (type.IsGenericParameter)
            {
                return $"``{type.GenericParameterPosition}";
            }

            if (type.IsGenericType)
            {
                Type genericType = type.GetGenericTypeDefinition();
                string typeName = $"{genericType.Namespace}.{ConformTypeShortName(genericType)}";
                string genericArgs = string.Join(",", type.GetGenericArguments().Select((_, index) => $"`{index}"));
                return $"{typeName}`{type.GetGenericArguments().Length}[{genericArgs}]";
            }

            return $"{type.Namespace}.{ConformTypeShortName(type)}";
        }
    }

}
