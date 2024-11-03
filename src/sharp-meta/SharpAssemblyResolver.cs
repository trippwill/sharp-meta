using System.Reflection;

namespace SharpMeta;

/// <summary>
/// Resolves assemblies from specified paths.
/// </summary>
public partial class SharpAssemblyResolver : PathAssemblyResolver
{
    /// <summary>
    /// Initializes a new instance of the <see cref="SharpAssemblyResolver"/> class.
    /// </summary>
    /// <param name="assemblyPaths">The paths to the assemblies.</param>
    private SharpAssemblyResolver(IEnumerable<string> assemblyPaths)
        : base(assemblyPaths)
    {
    }

    /// <summary>
    /// Creates a new <see cref="Builder"/> instance.
    /// </summary>
    /// <param name="logger">The logger to use for load context events.</param>
    /// <returns>A new <see cref="Builder"/> instance.</returns>
    public static Builder CreateBuilder(SharpResolverLogger? logger = null) => new(logger);

    /// <summary>
    /// Creates a <see cref="MetadataLoadContext"/> for the executing assembly.
    /// </summary>
    /// <returns>A new <see cref="MetadataLoadContext"/> instance.</returns>
    public static MetadataLoadContext CreateExecutingAssemblyLoadContext() => CreateBuilder()
        .AddExecutingAssembly(includeSideBySideAssemblies: true)
        .AddRuntimeEnvironmentAssemblies()
        .ToAssemblyResolver()
        .ToMetadataLoadContext();

    /// <summary>
    /// Implicitly converts a <see cref="Builder"/> to a <see cref="SharpAssemblyResolver"/>.
    /// </summary>
    /// <param name="builder">The builder to convert.</param>
    public static implicit operator SharpAssemblyResolver(Builder builder)
    {
        ArgumentNullException.ThrowIfNull(builder);
        return builder.ToAssemblyResolver();
    }

    /// <summary>
    /// Implicitly converts a <see cref="SharpAssemblyResolver"/> to a <see cref="MetadataLoadContext"/>.
    /// </summary>
    /// <param name="resolver">The resolver to convert.</param>
    public static implicit operator MetadataLoadContext(SharpAssemblyResolver resolver)
    {
        ArgumentNullException.ThrowIfNull(resolver);
        return resolver.ToMetadataLoadContext();
    }

    /// <summary>
    /// Converts the resolver to a <see cref="MetadataLoadContext"/>.
    /// </summary>
    /// <returns>A new <see cref="MetadataLoadContext"/> instance.</returns>
    public MetadataLoadContext ToMetadataLoadContext() => new(this);
}
