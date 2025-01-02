using System.Collections.Immutable;
using System.Diagnostics;

namespace SharpMeta;

/// <summary>
/// Represents the documentation comments for a member.
/// </summary>
[DebuggerDisplay($"{{{nameof(GetDebuggerDisplay)}(),nq}}")]
public record DocComments(
    string? Summary,
    ImmutableArray<(string Name, string Value)> Parameters,
    string? Returns)
{
    private string GetDebuggerDisplay()
    {
        return $"({this.Summary}) [{this.Parameters.Length}] {{{this.Returns}}}";
    }
}
