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
    public void AddReferenceFile_ShouldAddAssemblyFromFile()
    {
        SharpAssemblyResolver.Builder builder = SharpAssemblyResolver.CreateBuilder(_logger);
        var file = new FileInfo(Assembly.GetExecutingAssembly().Location);
        builder.AddReferenceFile(file);
        SharpAssemblyResolver resolver = builder.ToAssemblyResolver();
        Assert.NotNull(resolver);
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

    private class TestLogger : SharpResolverLogger
    {
        public List<string> Warnings { get; } = new List<string>();

        public TestLogger()
        {
            this.OnWarning = message => this.Warnings.Add(message);
        }
    }
}
