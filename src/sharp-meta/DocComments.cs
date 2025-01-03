using System.Collections.Immutable;
using System.Diagnostics;

namespace SharpMeta;

/// <summary>
/// Represents the documentation comments for a member.
/// </summary>
/// <param name="Summary">The summary of the documentation comments.</param>
/// <param name="Remarks">Additional remarks about the member.</param>
/// <param name="Example">An example demonstrating the usage of the member.</param>
/// <param name="Returns">The return value documentation of the member.</param>
/// <param name="Parameters">The parameters of the member, represented as an immutable array of name-value pairs.</param>
/// <param name="TypeParameters">The type parameters of the member, represented as an immutable array of name-value pairs.</param>
/// <param name="Exceptions">The exceptions that the member can throw, represented as an immutable array of name-value pairs.</param>
[DebuggerDisplay($"{{{nameof(GetDebuggerDisplay)}(),nq}}")]
public record DocComments(
    string? Summary,
    string? Remarks,
    string? Example,
    string? Returns,
    ImmutableArray<(string Name, string Value)> Parameters,
    ImmutableArray<(string Name, string Value)> TypeParameters,
    ImmutableArray<(string Name, string Value)> Exceptions)
{
    private string GetDebuggerDisplay()
    {
        return $"({this.Summary}) [{this.Parameters.Length}] {{{this.Returns}}}";
    }
}
