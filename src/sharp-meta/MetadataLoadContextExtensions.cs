using System.Reflection;

namespace SharpMeta;

/// <summary>
/// Extension methods for the MetadataLoadContext class.
/// </summary>
public static class MetadataLoadContextExtensions
{
    /// <summary>
    /// Loads an assembly from the specified file into the given MetadataLoadContext.
    /// </summary>
    /// <param name="context">The MetadataLoadContext to load the assembly into.</param>
    /// <param name="file">The file containing the assembly to load.</param>
    /// <returns>The loaded assembly.</returns>
    /// <exception cref="ArgumentNullException">Thrown if the context or file is null.</exception>
    public static Assembly LoadAssembly(this MetadataLoadContext context, FileInfo file)
    {
        ArgumentNullException.ThrowIfNull(context);
        ArgumentNullException.ThrowIfNull(file);

        return context.LoadFromAssemblyPath(file.FullName);
    }
}
