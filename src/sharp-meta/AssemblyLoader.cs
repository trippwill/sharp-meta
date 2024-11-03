using System.Reflection;
using System.Runtime.InteropServices;

namespace SharpMeta;

/// <summary>
/// Initializes a new instance of the <see cref="AssemblyLoader"/> class.
/// </summary>
/// <param name="logAction">The action for error output.</param>
/// <param name="directoryRecursionDepth">The maximum recursion depth when searching for reference files in directories.</param>
/// <param name="referenceFiles">A collection of reference files.</param>
/// <param name="referenceDirectories">A collection of reference directories.</param>
/// <param name="includeExecutingCoreAssembly">Attempt to add the core assembly path of the executing assembly to the load context.</param>
/// <param name="includeExecutingRuntimeAssemblies">Attempt to add the runtime assemblies of the executing assembly to the load context.</param>
public sealed class AssemblyLoader(
    Action<string>? logAction,
    int directoryRecursionDepth,
    FileInfo[] referenceFiles,
    DirectoryInfo[] referenceDirectories,
    bool includeExecutingCoreAssembly = false,
    bool includeExecutingRuntimeAssemblies = false) : IDisposable
{
    private readonly MetadataLoadContext _context = GetContext(
        logAction,
        directoryRecursionDepth,
        referenceFiles,
        referenceDirectories,
        includeExecutingCoreAssembly,
        includeExecutingRuntimeAssemblies);

    /// <summary>
    /// Gets the internal metadata load context.
    /// </summary>
    public MetadataLoadContext LoadContext => _context;

    /// <inheritdoc/>
    public void Dispose() => _context.Dispose();

    /// <summary>
    /// Loads an assembly from the specified file path.
    /// </summary>
    /// <param name="assemblyFile">The file path of the assembly to load.</param>
    /// <returns>The loaded assembly.</returns>
    public Assembly LoadAssembly(FileInfo assemblyFile)
    {
        ArgumentNullException.ThrowIfNull(assemblyFile);

        return _context.LoadFromAssemblyPath(assemblyFile.FullName);
    }

    private static MetadataLoadContext GetContext(
        Action<string>? logAction,
        int directoryRecursionDepth,
        ReadOnlyMemory<FileInfo> referenceFiles,
        ReadOnlyMemory<DirectoryInfo> referenceDirectories,
        bool includeCoreAssembly,
        bool includeRuntimeAssemblies)
    {
        Dictionary<string, string> assemblyNamePathMap = new(StringComparer.OrdinalIgnoreCase);

        if (includeCoreAssembly)
        {
            string coreAssemblyPath = typeof(object).Assembly.Location;
            string? coreAssemblyName = Path.GetFileName(coreAssemblyPath);
            if (coreAssemblyName is not null)
            {
                if (!File.Exists(coreAssemblyPath))
                {
                    logAction?.Invoke($"Core assembly not found: {coreAssemblyPath}\n");
                }
                else
                {
                    assemblyNamePathMap[coreAssemblyName] = coreAssemblyPath;
                }
            }
            else
            {
                logAction?.Invoke("Failed to get core assembly name.\n");
            }
            assemblyNamePathMap[Path.GetFileName(coreAssemblyPath)] = coreAssemblyPath;
        }

        if (includeRuntimeAssemblies)
        {
            string[] runtimeAssemblies = Directory.GetFiles(RuntimeEnvironment.GetRuntimeDirectory(), "*.dll");
            foreach (string runtimeAssembly in runtimeAssemblies)
            {
                string? runtimeAssemblyName = Path.GetFileName(runtimeAssembly);
                if (runtimeAssemblyName is not null)
                {
                    if (!File.Exists(runtimeAssembly))
                    {
                        logAction?.Invoke($"Runtime assembly not found: {runtimeAssembly}\n");
                    }
                    else
                    {
                        assemblyNamePathMap[runtimeAssemblyName] = runtimeAssembly;
                    }
                }
                else
                {
                    logAction?.Invoke("Failed to get runtime assembly name.\n");
                }
            }
        }

        foreach (FileInfo referenceFile in referenceFiles.Span)
        {
            if (!referenceFile.Exists)
            {
                logAction?.Invoke($"Assembly not found: {referenceFile.FullName}\n");
                continue;
            }

            if (assemblyNamePathMap.ContainsKey(referenceFile.Name))
            {
                continue;
            }

            assemblyNamePathMap[referenceFile.Name] = referenceFile.FullName;
        }

        foreach (DirectoryInfo referenceDirectory in referenceDirectories.Span)
        {
            if (!referenceDirectory.Exists)
            {
                logAction?.Invoke($"Directory not found: {referenceDirectory.FullName}\n");
                continue;
            }

            foreach (string path in Directory.EnumerateFiles(referenceDirectory.FullName, "*.dll", new EnumerationOptions
            {
                RecurseSubdirectories = true,
                MaxRecursionDepth = directoryRecursionDepth,
            }))
            {
                if (assemblyNamePathMap.ContainsKey(Path.GetFileName(path)))
                {
                    continue;
                }

                assemblyNamePathMap[Path.GetFileName(path)] = path;
            }
        }

        PathAssemblyResolver resolver = new(assemblyNamePathMap.Values);
        return new(resolver);
    }
}
