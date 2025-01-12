using System.Reflection;
using SharpMeta;
using Xunit;
using Xunit.Abstractions;

namespace Tests.SharpAssemblyResolverBuilder;

public class TestBase(ITestOutputHelper output)
{
    public SharpResolverLogger Logger { get; } = output.ToSharpResolverLogger();
}

public class ToAssemblyResolver(ITestOutputHelper output) : TestBase(output)
{
    [Fact]
    public void ShouldReturnSharpAssemblyResolverInstance()
    {
        SharpAssemblyResolver.Builder builder = SharpAssemblyResolver.CreateBuilder(this.Logger);
        SharpAssemblyResolver resolver = builder.ToAssemblyResolver();
        Assert.NotNull(resolver);
        Assert.IsType<SharpAssemblyResolver>(resolver);
    }
}

public class AddReferenceDirectory(ITestOutputHelper output) : TestBase(output)
{
    [Fact]
    public void ShouldAddAssembliesFromDirectory()
    {
        SharpAssemblyResolver.Builder builder = SharpAssemblyResolver.CreateBuilder(this.Logger);
        var directory = new DirectoryInfo(Directory.GetCurrentDirectory());
        builder.AddReferenceDirectory(directory);
        SharpAssemblyResolver resolver = builder.ToAssemblyResolver();
        Assert.NotNull(resolver);
    }

    [Fact]
    public void ShouldThrowArgumentNullException_WhenDirectoryIsNull()
    {
        SharpAssemblyResolver.Builder builder = SharpAssemblyResolver.CreateBuilder(this.Logger);
        Assert.Throws<ArgumentNullException>(() => builder.AddReferenceDirectory(null!));
    }
}

public class AddReferenceDirectories(ITestOutputHelper output) : TestBase(output)
{
    [Fact]
    public void ShouldAddAssembliesFromMultipleDirectories()
    {
        SharpAssemblyResolver.Builder builder = SharpAssemblyResolver.CreateBuilder(this.Logger);
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
    public void ShouldThrowArgumentNullException_WhenDirectoriesAreNull()
    {
        SharpAssemblyResolver.Builder builder = SharpAssemblyResolver.CreateBuilder(this.Logger);
        Assert.Throws<ArgumentNullException>(() => builder.AddReferenceDirectories(new EnumerationOptions(), null!));
    }

    [Fact]
    public void ShouldLogWarning_WhenDirectoryDoesNotExist()
    {
        var logger = new TestLogger();
        SharpAssemblyResolver.Builder builder = SharpAssemblyResolver.CreateBuilder(logger);
        var directory = new DirectoryInfo("NonExistentDirectory");
        builder.AddReferenceDirectory(directory);
        Assert.Contains($"Directory not found: {directory.FullName}.", logger.Warnings);
    }
}

public class AddReferenceFile(ITestOutputHelper output) : TestBase(output)
{
    [Fact]
    public void ShouldAddAssemblyFromFile()
    {
        SharpAssemblyResolver.Builder builder = SharpAssemblyResolver.CreateBuilder(this.Logger);
        var file = new FileInfo(Assembly.GetExecutingAssembly().Location);
        builder.AddReferenceFile(file);
        SharpAssemblyResolver resolver = builder.ToAssemblyResolver();
        Assert.NotNull(resolver);
    }

    [Fact]
    public void ShouldThrowArgumentNullException_WhenFileIsNull()
    {
        SharpAssemblyResolver.Builder builder = SharpAssemblyResolver.CreateBuilder(this.Logger);
        Assert.Throws<ArgumentNullException>(() => builder.AddReferenceFile(null!));
    }

    [Fact]
    public void ShouldIncludeSideBySideAssemblies()
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
        Assert.Contains("Added assembly:", string.Join(' ', logger.Infos));
    }

    [Fact]
    public void ShouldLogError_WhenFileDirectoryNotFound()
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
}

public class AddReferenceFiles(ITestOutputHelper output) : TestBase(output)
{
    [Fact]
    public void ShouldAddAssembliesFromMultipleFiles()
    {
        SharpAssemblyResolver.Builder builder = SharpAssemblyResolver.CreateBuilder(this.Logger);
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
    public void ShouldThrowArgumentNullException_WhenFilesAreNull()
    {
        SharpAssemblyResolver.Builder builder = SharpAssemblyResolver.CreateBuilder(this.Logger);
        Assert.Throws<ArgumentNullException>(() => builder.AddReferenceFiles(null!));
    }
}

public class AddExecutingAssembly(ITestOutputHelper output) : TestBase(output)
{
    [Fact]
    public void ShouldAddExecutingAssembly()
    {
        SharpAssemblyResolver.Builder builder = SharpAssemblyResolver.CreateBuilder(this.Logger);
        builder.AddExecutingAssembly();
        SharpAssemblyResolver resolver = builder.ToAssemblyResolver();
        Assert.NotNull(resolver);
    }
}

public class AddRuntimeEnvironmentAssemblies(ITestOutputHelper output) : TestBase(output)
{
    [Fact]
    public void ShouldAddRuntimeAssemblies()
    {
        SharpAssemblyResolver.Builder builder = SharpAssemblyResolver.CreateBuilder(this.Logger);
        builder.AddRuntimeEnvironmentAssemblies();
        SharpAssemblyResolver resolver = builder.ToAssemblyResolver();
        Assert.NotNull(resolver);
    }
}

public class AddAssembly
{
    [Fact]
    public void ShouldLogWarning_WhenFileDoesNotExist()
    {
        var logger = new TestLogger();
        SharpAssemblyResolver.Builder builder = SharpAssemblyResolver.CreateBuilder(logger);
        var file = new FileInfo("NonExistentFile.dll");
        builder.AddReferenceFile(file);
        Assert.Contains($"File not found: {file.FullName}.", logger.Warnings);
    }

    [Fact]
    public void ShouldLogWarning_WhenAssemblyPathIsOverwritten()
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
    public void ShouldNotLogWarning_WhenAssemblyPathIsNotOverwritten()
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
}

internal class TestLogger : SharpResolverLogger
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