using System.Reflection;
using System.Runtime.InteropServices;

namespace SharpMeta;

public partial class SharpAssemblyResolver
{
    /// <summary>
    /// Provides a builder for creating <see cref="SharpAssemblyResolver"/> instances.
    /// </summary>
    public class Builder
    {
        private readonly SharpResolverLogger _logger;
        private readonly HashSet<string> _assemblyPaths = new(comparer: StringComparer.OrdinalIgnoreCase);

        /// <summary>
        /// Initializes a new instance of the <see cref="Builder"/> class.
        /// </summary>
        /// <param name="logger">The logger to use for load context events.</param>
        internal Builder(SharpResolverLogger? logger = null)
        {
            _logger = logger ?? SharpResolverLogger.Default;
        }

        /// <summary>
        /// Converts the builder to a <see cref="SharpAssemblyResolver"/>.
        /// </summary>
        /// <returns>A new <see cref="SharpAssemblyResolver"/> instance.</returns>
        public SharpAssemblyResolver ToAssemblyResolver()
        {
            return new SharpAssemblyResolver(_assemblyPaths);
        }

        /// <summary>
        /// Adds a reference directory to the builder.
        /// </summary>
        /// <param name="directory">The directory to add.</param>
        /// <param name="options">The enumeration options to use.</param>
        /// <returns>The current <see cref="Builder"/> instance.</returns>
        /// <exception cref="ArgumentNullException">Thrown when the directory is null.</exception>
        public Builder AddReferenceDirectory(DirectoryInfo directory, EnumerationOptions? options = null)
        {
            ArgumentNullException.ThrowIfNull(directory);

            options ??= new EnumerationOptions
            {
                RecurseSubdirectories = false,
            };

            return this.AddAssembliesFromDirectory(directory, options);
        }

        /// <summary>
        /// Adds multiple reference directories to the builder.
        /// </summary>
        /// <param name="options">The enumeration options to use.</param>
        /// <param name="directories">The directories to add.</param>
        /// <returns>The current <see cref="Builder"/> instance.</returns>
        /// <exception cref="ArgumentNullException">Thrown when the directories are null.</exception>
        public Builder AddReferenceDirectories(EnumerationOptions options, params DirectoryInfo[] directories)
        {
            ArgumentNullException.ThrowIfNull(directories);

            foreach (DirectoryInfo directory in directories)
            {
                this.AddAssembliesFromDirectory(directory, options);
            }

            return this;
        }

        /// <summary>
        /// Adds a reference file to the builder.
        /// </summary>
        /// <param name="file">The file to add.</param>
        /// <param name="includeSideBySideAssemblies">Whether to include side-by-side assemblies.</param>
        /// <returns>The current <see cref="Builder"/> instance.</returns>
        /// <exception cref="ArgumentNullException">Thrown when the file is null.</exception>
        public Builder AddReferenceFile(FileInfo file, bool includeSideBySideAssemblies = false)
        {
            ArgumentNullException.ThrowIfNull(file);

            if (includeSideBySideAssemblies)
            {
                string? fileDirectory = Path.GetDirectoryName(file.FullName);
                if (fileDirectory is null)
                {
                    _logger.OnError?.Invoke($"File directory not found: '{fileDirectory}'");
                    return this;
                }

                return this.AddReferenceDirectories(
                    new EnumerationOptions
                    {
                        RecurseSubdirectories = true,
                        MaxRecursionDepth = 1,
                    },
                    new DirectoryInfo(fileDirectory));
            }

            return this.AddAssembly(file);
        }

        /// <summary>
        /// Adds multiple reference files to the builder.
        /// </summary>
        /// <param name="files">The files to add.</param>
        /// <returns>The current <see cref="Builder"/> instance.</returns>
        /// <exception cref="ArgumentNullException">Thrown when the files are null.</exception>
        public Builder AddReferenceFiles(params FileInfo[] files)
        {
            ArgumentNullException.ThrowIfNull(files);

            foreach (FileInfo file in files)
            {
                this.AddAssembly(file);
            }

            return this;
        }

        /// <summary>
        /// Adds the executing assembly to the builder.
        /// </summary>
        /// <param name="includeSideBySideAssemblies">Whether to include side-by-side assemblies.</param>
        /// <param name="sideBySideRecursionDepth">The recursion depth for side-by-side assemblies.</param>
        /// <returns>The current <see cref="Builder"/> instance.</returns>
        public Builder AddExecutingAssembly(bool includeSideBySideAssemblies = false, int sideBySideRecursionDepth = 1)
        {
            string executingAssemblyLocation = Assembly.GetExecutingAssembly().Location;

            if (includeSideBySideAssemblies)
            {
                string? executingAssemblyDirectory = Path.GetDirectoryName(executingAssemblyLocation);
                if (executingAssemblyDirectory is null)
                {
                    _logger.OnError?.Invoke("Executing assembly directory not found.");
                    return this;
                }

                this.AddReferenceDirectories(
                    new EnumerationOptions
                    {
                        RecurseSubdirectories = true,
                        MaxRecursionDepth = sideBySideRecursionDepth,
                    },
                    new DirectoryInfo(executingAssemblyDirectory));

                return this;
            }

            return this.AddAssembly(new FileInfo(executingAssemblyLocation));
        }

        /// <summary>
        /// Adds the runtime assemblies of the executing application to the builder.
        /// </summary>
        /// <returns>The current <see cref="Builder"/> instance.</returns>
        public Builder AddRuntimeEnvironmentAssemblies()
        {
            string[] runtimeAssemblies = Directory.GetFiles(RuntimeEnvironment.GetRuntimeDirectory(), "*.dll");

            foreach (string runtimeAssembly in runtimeAssemblies)
            {
                this.AddAssembly(new FileInfo(runtimeAssembly));
            }

            return this;
        }

        /// <summary>
        /// Adds assemblies from a directory to the builder.
        /// </summary>
        /// <param name="directory">The directory to add assemblies from.</param>
        /// <param name="options">The enumeration options to use.</param>
        /// <returns>The current <see cref="Builder"/> instance.</returns>
        private Builder AddAssembliesFromDirectory(DirectoryInfo directory, EnumerationOptions options)
        {
            if (!directory.Exists)
            {
                _logger.OnWarning?.Invoke($"Directory not found: {directory.FullName}.");
                return this;
            }

            foreach (FileInfo file in directory.EnumerateFiles("*.dll", options))
            {
                this.AddAssembly(file);
            }

            return this;
        }

        /// <summary>
        /// Adds an assembly to the builder.
        /// </summary>
        /// <param name="file">The file to add.</param>
        /// <returns>The current <see cref="Builder"/> instance.</returns>
        private Builder AddAssembly(FileInfo file)
        {
            if (!file.Exists)
            {
                _logger.OnWarning?.Invoke($"File not found: {file.FullName}.");
                return this;
            }

            _assemblyPaths.Add(file.FullName);
            _logger.OnInfo?.Invoke($"Added assembly: {file.FullName}.");

            return this;
        }
    }
}
