using System.Collections.Concurrent;
using System.Collections.Immutable;
using System.Reflection;
using System.Security;
using System.Text.RegularExpressions;
using System.Xml.Linq;

using ValuePair = (string Name, string Value);

namespace SharpMeta;

/// <summary>
/// Represents the documentation comments for a member.
/// </summary>
/// <param name="Summary">The summary of the member.</param>
/// <param name="Remarks">The remarks for the member.</param>
/// <param name="Returns">The return value of the member.</param>
/// <param name="Parameters">The parameters of the member.</param>
/// <param name="TypeParameters">The type parameters of the member.</param>
/// <param name="Exceptions">The exceptions thrown by the member.</param>
/// <param name="Examples">Examples of how to use the member.</param>
public partial record DocComments(
    string? Summary,
    string? Remarks,
    string? Returns,
    ImmutableArray<ValuePair> Parameters,
    ImmutableArray<ValuePair> TypeParameters,
    ImmutableArray<ValuePair> Exceptions,
    ImmutableArray<string> Examples)
{
    /// <summary>
    /// XDocument by file path cache.
    /// </summary>
    private static readonly ConcurrentDictionary<string, XDocument?> XmlDocumentationCache = new(StringComparer.OrdinalIgnoreCase);

    /// <summary>
    /// Gets or sets the maximum depth for parsing XML documentation.
    /// </summary>
    public static int MaxDepth { get; set; } = 24;

    /// <summary>
    /// Parses the XML documentation comments for a given member.
    /// </summary>
    /// <param name="memberInfo">The <see cref="MemberInfo"/> to parse the documentation comments for.</param>
    /// <returns>A <see cref="DocComments"/> instance containing the parsed documentation comments.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="memberInfo"/> is <see langword="null"/>.</exception>
    public static DocComments? Parse(MemberInfo memberInfo) => Parse(memberInfo, new DepthScope(MaxDepth));

    /// <summary>
    /// Normalizes the documentation comments for human readability.
    /// </summary>
    /// <returns>A new <see cref="DocComments"/> instance with normalized values.</returns>
    public DocComments Normalize()
    {
        return new DocComments(
            Summary is not null ? NormalizeString(Summary) : Summary,
            Remarks is not null ? NormalizeString(Remarks) : Remarks,
            Returns is not null ? NormalizeString(Returns) : Returns,
            [.. Parameters.Select(p => (p.Name, NormalizeString(p.Value)))],
            [.. TypeParameters.Select(tp => (tp.Name, NormalizeString(tp.Value)))],
            [.. Exceptions.Select(e => (e.Name, NormalizeString(e.Value)))],
            [.. Examples.Select(NormalizeString)]);
    }

    private static DocComments? Parse(MemberInfo memberInfo, DepthScope depthScope)
    {
        using DepthScope.Releaser _ = depthScope.EnterScope();

        ArgumentNullException.ThrowIfNull(memberInfo, nameof(memberInfo));

        XDocument? xmlDoc = LoadXmlDocumentation(memberInfo);
        if (xmlDoc is null)
            return null;

        // Create the member name in the format used in the XML documentation file
        string memberName = memberInfo.GetDocId();

        // Find the documentation element for the member
        XElement? memberElement = xmlDoc.Descendants("member")
            .FirstOrDefault(e => e.Attribute("name")?.Value == memberName);

        if (memberElement is null)
            return null;

        string? summary = memberElement.Element("summary")?.Value.Trim();
        string? returns = memberElement.Element("returns")?.Value.Trim();
        string? remarks = memberElement.Element("remarks")?.Value.Trim();

        var examples = memberElement.Elements("example")
            .Select(e => e.Value.Trim())
            .ToImmutableArray();

        var parameters = memberElement.Elements("param")
            .Select(e => (e.Attribute("name")?.Value ?? string.Empty, e.Value.Trim()))
            .ToImmutableArray();

        var exceptions = memberElement.Elements("exception")
            .Select(e => (e.Attribute("cref")?.Value.Split(':').ElementAtOrDefault(1) ?? string.Empty, e.Value.Trim()))
            .ToImmutableArray();

        var typeParameters = memberElement.Elements("typeparam")
            .Select(e => (e.Attribute("name")?.Value ?? string.Empty, e.Value.Trim()))
            .ToImmutableArray();

        // Handle inheritdoc
        XElement? inheritdocElement = memberElement.Element("inheritdoc");
        if (inheritdocElement is not null)
        {
            DocComments? baseComments = GetInheritedComments(memberInfo, inheritdocElement, xmlDoc, depthScope);
            if (baseComments is not null)
            {
                summary ??= baseComments.Summary;
                remarks ??= baseComments.Remarks;
                examples = examples.IsDefaultOrEmpty ? baseComments.Examples : examples;
                returns ??= baseComments.Returns;
                parameters = parameters.IsDefaultOrEmpty ? baseComments.Parameters : parameters;
                exceptions = exceptions.IsDefaultOrEmpty ? baseComments.Exceptions : exceptions;
                typeParameters = typeParameters.IsDefaultOrEmpty ? baseComments.TypeParameters : typeParameters;
            }
        }

        return new DocComments(
            summary,
            remarks,
            returns,
            parameters,
            typeParameters,
            exceptions,
            examples);
    }

    /// <summary>
    /// Loads the XML documentation for a given member.
    /// </summary>
    /// <param name="memberInfo">The <see cref="MemberInfo"/> to load the documentation for.</param>
    /// <returns>An <see cref="XDocument"/> containing the XML documentation, or <see langword="null"/> if the documentation could not be loaded.</returns>
    private static XDocument? LoadXmlDocumentation(MemberInfo memberInfo)
    {
        // Get the XML documentation file path
        string xmlDocumentationPath = Path.ChangeExtension(memberInfo.Module.Assembly.Location, ".xml");

        // Check the cache for the XDocument or load it from the file
        return XmlDocumentationCache.GetOrAdd(xmlDocumentationPath, path =>
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
    }

    private static DocComments? GetInheritedComments(
        MemberInfo memberInfo,
        XElement inheritdocElement,
        XDocument xDoc,
        DepthScope depthScope)
    {
        // Check base class recursively
        if (memberInfo.DeclaringType?.BaseType is not null)
        {
            MemberInfo? baseMember = memberInfo.DeclaringType.BaseType.GetMember(memberInfo.Name).FirstOrDefault();
            if (baseMember is not null)
            {
                DocComments? baseComments = Parse(baseMember, depthScope);
                if (baseComments is not null)
                    return baseComments;
            }
        }

        // Check interfaces recursively
        foreach (Type interfaceType in memberInfo.DeclaringType?.GetInterfaces() ?? Enumerable.Empty<Type>())
        {
            MemberInfo? interfaceMember = interfaceType.GetMember(memberInfo.Name).FirstOrDefault();
            if (interfaceMember is not null)
            {
                DocComments? interfaceComments = Parse(interfaceMember, depthScope);
                if (interfaceComments is not null)
                    return interfaceComments;
            }
        }

        // Check cref attribute
        string? cref = inheritdocElement.Attribute("cref")?.Value;
        if (!string.IsNullOrEmpty(cref))
        {
            XElement? crefElement = xDoc.Descendants("member")
                .FirstOrDefault(e => e.Attribute("name")?.Value == cref);

            if (crefElement is not null)
            {
                string? summary = crefElement.Element("summary")?.Value.Trim();
                string? returns = crefElement.Element("returns")?.Value.Trim();
                string? remarks = crefElement.Element("remarks")?.Value.Trim();
                var examples = crefElement.Elements("example")
                    .Select(e => e.Value.Trim())
                    .ToImmutableArray();

                var parameters = crefElement.Elements("param")
                    .Select(e => (e.Attribute("name")?.Value ?? string.Empty, e.Value.Trim()))
                    .ToImmutableArray();

                var exceptions = crefElement.Elements("exception")
                    .Select(e => (e.Attribute("cref")?.Value.Split(':').ElementAtOrDefault(1) ?? string.Empty, e.Value.Trim()))
                    .ToImmutableArray();

                var typeParameters = crefElement.Elements("typeparam")
                    .Select(e => (e.Attribute("name")?.Value ?? string.Empty, e.Value.Trim()))
                    .ToImmutableArray();

                return new DocComments(
                    summary,
                    remarks,
                    returns,
                    parameters,
                    typeParameters,
                    exceptions,
                    examples);
            }
        }

        return null;
    }

    [GeneratedRegex(@"<br\s*/?>", RegexOptions.IgnoreCase, "en-US")]
    internal static partial Regex BrTagRegex();
    
    [GeneratedRegex(@"</para\s*>", RegexOptions.IgnoreCase, "en-US")]
    internal static partial Regex EndParaTagRegex();
    
    [GeneratedRegex(@"<para\s*>", RegexOptions.IgnoreCase, "en-US")]
    internal static partial Regex StartParaTagRegex();
    
    [GeneratedRegex(@"<[^>]+>")]
    internal static partial Regex XmlTagRegex();
    
    [GeneratedRegex(@"\s*\n\s*")]
    internal static partial Regex WhitespaceNewLineRegex();

    private static string NormalizeString(string input)
    {
        // Replace <br/> and <br /> with new lines
        string result = BrTagRegex().Replace(input, "\n");

        // Replace <para> and </para> with double new lines
        result = StartParaTagRegex().Replace(result, string.Empty);
        result = EndParaTagRegex().Replace(result, "\n\n");

        // Remove other tags but preserve their content
        result = XmlTagRegex().Replace(result, string.Empty);

        // Normalize indentation
        result = WhitespaceNewLineRegex().Replace(result, "\n");

        return result.Trim();
    }
}
