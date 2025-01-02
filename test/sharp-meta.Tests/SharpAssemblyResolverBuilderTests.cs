using System.Reflection;
using Xunit;
using Xunit.Abstractions;

namespace SharpMeta.Tests;

public class SharpAssemblyResolverBuilderTests(ITestOutputHelper outputHelper)
{
    private readonly SharpResolverLogger _logger = outputHelper.ToSharpResolverLogger();

    [Fact]
    public void ToAssemblyResolver_ShouldReturnSharpAssemblyResolverInstance()
    {
        SharpAssemblyResolver.Builder builder = SharpAssemblyResolver.CreateBuilder(_logger);
        SharpAssemblyResolver resolver = builder.ToAssemblyResolver();
        Assert.NotNull(resolver);
        Assert.IsType<SharpAssemblyResolver>(resolver);
    }

    [Fact]
    public void AddReferenceDirectory_ShouldAddAssembliesFromDirectory()
    {
        SharpAssemblyResolver.Builder builder = SharpAssemblyResolver.CreateBuilder(_logger);
        var directory = new DirectoryInfo(Directory.GetCurrentDirectory());
        builder.AddReferenceDirectory(directory);
        SharpAssemblyResolver resolver = builder.ToAssemblyResolver();
        Assert.NotNull(resolver);
    }

    [Fact]
    public void AddReferenceDirectory_ShouldThrowArgumentNullException_WhenDirectoryIsNull()
    {
        SharpAssemblyResolver.Builder builder = SharpAssemblyResolver.CreateBuilder(_logger);
        Assert.Throws<ArgumentNullException>(() => builder.AddReferenceDirectory(null!));
    }

    [Fact]
    public void AddReferenceDirectories_ShouldAddAssembliesFromMultipleDirectories()
    {
        SharpAssemblyResolver.Builder builder = SharpAssemblyResolver.CreateBuilder(_logger);
        DirectoryInfo[] directories =
        [
            new DirectoryInfo(Directory.GetCurrentDirectory()),
            new DirectoryInfo(Path.GetTempPath())
        ];
        builder.AddReferenceDirectories(new EnumerationOptions(), directories);
        SharpAssemblyResolver resolver = builder.ToAssemblyResolver();
        Assert.NotNull(resolver);
    }

    [Fact]
    public void AddReferenceDirectories_ShouldThrowArgumentNullException_WhenDirectoriesAreNull()
    {
        SharpAssemblyResolver.Builder builder = SharpAssemblyResolver.CreateBuilder(_logger);
        Assert.Throws<ArgumentNullException>(() => builder.AddReferenceDirectories(new EnumerationOptions(), null!));
    }

    [Fact]
    public void AddReferenceFile_ShouldAddAssemblyFromFile()
    {
        SharpAssemblyResolver.Builder builder = SharpAssemblyResolver.CreateBuilder(_logger);
        var file = new FileInfo(Assembly.GetExecutingAssembly().Location);
        builder.AddReferenceFile(file);
        SharpAssemblyResolver resolver = builder.ToAssemblyResolver();
        Assert.NotNull(resolver);
    }

    [Fact]
    public void AddReferenceFile_ShouldThrowArgumentNullException_WhenFileIsNull()
    {
        SharpAssemblyResolver.Builder builder = SharpAssemblyResolver.CreateBuilder(_logger);
        Assert.Throws<ArgumentNullException>(() => builder.AddReferenceFile(null!));
    }

    [Fact]
    public void AddReferenceFiles_ShouldAddAssembliesFromMultipleFiles()
    {
        SharpAssemblyResolver.Builder builder = SharpAssemblyResolver.CreateBuilder(_logger);
        FileInfo[] files = new[]
        {
            new FileInfo(Assembly.GetExecutingAssembly().Location),
            new FileInfo(Assembly.GetCallingAssembly().Location)
        };
        builder.AddReferenceFiles(files);
        SharpAssemblyResolver resolver = builder.ToAssemblyResolver();
        Assert.NotNull(resolver);
    }

    [Fact]
    public void AddReferenceFiles_ShouldThrowArgumentNullException_WhenFilesAreNull()
    {
        SharpAssemblyResolver.Builder builder = SharpAssemblyResolver.CreateBuilder(_logger);
        Assert.Throws<ArgumentNullException>(() => builder.AddReferenceFiles(null!));
    }

    [Fact]
    public void AddExecutingAssembly_ShouldAddExecutingAssembly()
    {
        SharpAssemblyResolver.Builder builder = SharpAssemblyResolver.CreateBuilder(_logger);
        builder.AddExecutingAssembly();
        SharpAssemblyResolver resolver = builder.ToAssemblyResolver();
        Assert.NotNull(resolver);
    }

    [Fact]
    public void AddRuntimeEnvironmentAssemblies_ShouldAddRuntimeAssemblies()
    {
        SharpAssemblyResolver.Builder builder = SharpAssemblyResolver.CreateBuilder(_logger);
        builder.AddRuntimeEnvironmentAssemblies();
        SharpAssemblyResolver resolver = builder.ToAssemblyResolver();
        Assert.NotNull(resolver);
    }

    [Fact]
    public void AddAssembliesFromDirectory_ShouldLogWarning_WhenDirectoryDoesNotExist()
    {
        var logger = new TestLogger();
        SharpAssemblyResolver.Builder builder = SharpAssemblyResolver.CreateBuilder(logger);
        var directory = new DirectoryInfo("NonExistentDirectory");
        builder.AddReferenceDirectory(directory);
        Assert.Contains($"Directory not found: {directory.FullName}.", logger.Warnings);
    }

    [Fact]
    public void AddAssembly_ShouldLogWarning_WhenFileDoesNotExist()
    {
        var logger = new TestLogger();
        SharpAssemblyResolver.Builder builder = SharpAssemblyResolver.CreateBuilder(logger);
        var file = new FileInfo("NonExistentFile.dll");
        builder.AddReferenceFile(file);
        Assert.Contains($"File not found: {file.FullName}.", logger.Warnings);
    }

    [Fact]
    public void AddAssembly_ShouldLogWarning_WhenAssemblyPathIsOverwritten()
    {
        // Arrange
        var logger = new TestLogger();
        SharpAssemblyResolver.Builder builder = SharpAssemblyResolver.CreateBuilder(logger);

        var tempFilePath = Path.Combine(Path.GetTempPath(), "test.dll");
        File.WriteAllText(tempFilePath, ""); // Create an empty file
        var file = new FileInfo(tempFilePath);

        // Act
        builder.AddReferenceFile(file);
        builder.AddReferenceFile(file); // Add the same file again to trigger the warning

        // Assert
        Assert.Contains($"Overwriting existing assembly path: {file.FullName}.", logger.Warnings);
    }

    [Fact]
    public void AddAssembly_ShouldNotLogWarning_WhenAssemblyPathIsNotOverwritten()
    {
        // Arrange
        var logger = new TestLogger();
        SharpAssemblyResolver.Builder builder = SharpAssemblyResolver.CreateBuilder(logger);
        var file1 = new FileInfo("test1.dll");
        var file2 = new FileInfo("test2.dll");

        // Act
        builder.AddReferenceFile(file1);
        builder.AddReferenceFile(file2); // Add a different file

        // Assert
        Assert.DoesNotContain($"Overwriting existing assembly path: {file1.FullName}.", logger.Warnings);
        Assert.DoesNotContain($"Overwriting existing assembly path: {file2.FullName}.", logger.Warnings);
    }

    [Fact]
    public void AddReferenceFile_ShouldIncludeSideBySideAssemblies()
    {
        // Arrange
        var logger = new TestLogger();
        SharpAssemblyResolver.Builder builder = SharpAssemblyResolver.CreateBuilder(logger);
        var file = new FileInfo(Assembly.GetExecutingAssembly().Location);

        // Act
        builder.AddReferenceFile(file, includeSideBySideAssemblies: true);
        SharpAssemblyResolver resolver = builder.ToAssemblyResolver();

        // Assert
        Assert.NotNull(resolver);
        Assert.Contains("Added assembly:", string.Join(' ',logger.Infos));
    }

    [Fact]
    public void AddReferenceFile_ShouldLogError_WhenFileDirectoryNotFound()
    {
        // Arrange
        var logger = new TestLogger();
        SharpAssemblyResolver.Builder builder = SharpAssemblyResolver.CreateBuilder(logger);
        var file = new FileInfo("NonExistentFile.dll");

        // Act
        builder.AddReferenceFile(file, includeSideBySideAssemblies: false);

        // Assert
        Assert.Contains("File not found:", string.Join(' ', logger.Warnings));
    }

    private class TestLogger : SharpResolverLogger
    {
        public List<string> Infos { get; } = new List<string>();
        public List<string> Warnings { get; } = new List<string>();
        public List<string> Errors { get; } = new List<string>();

        public TestLogger()
        {
            this.OnInfo = message => this.Infos.Add(message);
            this.OnWarning = message => this.Warnings.Add(message);
            this.OnError = message => this.Errors.Add(message);
        }
    }
}
