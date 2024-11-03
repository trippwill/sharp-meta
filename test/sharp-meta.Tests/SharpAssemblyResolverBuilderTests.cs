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
}
