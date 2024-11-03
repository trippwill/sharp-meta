using System.Reflection;
using Xunit;
using Xunit.Abstractions;

namespace SharpMeta.Tests;

public class SharpAssemblyResolverTests(ITestOutputHelper outputHelper)
{
    [Fact]
    public void CreateBuilder_ShouldReturnBuilderInstance()
    {
        // Act
        SharpAssemblyResolver.Builder builder = SharpAssemblyResolver.CreateBuilder();

        // Assert
        Assert.NotNull(builder);
        Assert.IsType<SharpAssemblyResolver.Builder>(builder);
    }

    [Fact]
    public void CreateExecutingAssemblyLoadContext_ShouldReturnMetadataLoadContext()
    {
        // Act
        MetadataLoadContext loadContext = SharpAssemblyResolver.CreateExecutingAssemblyLoadContext();

        // Assert
        Assert.NotNull(loadContext);
        Assert.IsType<MetadataLoadContext>(loadContext);
    }

    [Fact]
    public void ImplicitConversion_FromBuilderToSharpAssemblyResolver_ShouldReturnSharpAssemblyResolver()
    {
        // Arrange
        SharpAssemblyResolver.Builder builder = SharpAssemblyResolver.CreateBuilder();

        // Act
        SharpAssemblyResolver resolver = builder;

        // Assert
        Assert.NotNull(resolver);
        Assert.IsType<SharpAssemblyResolver>(resolver);
    }

    [Fact]
    public void ImplicitConversion_FromSharpAssemblyResolverToMetadataLoadContext_ShouldReturnMetadataLoadContext()
    {
        // Arrange
        SharpAssemblyResolver.Builder builder = SharpAssemblyResolver
            .CreateBuilder()
            .AddRuntimeEnvironmentAssemblies();
        SharpAssemblyResolver resolver = builder;

        // Act
        MetadataLoadContext loadContext = resolver;

        // Assert
        Assert.NotNull(loadContext);
        Assert.IsType<MetadataLoadContext>(loadContext);
    }

    [Fact]
    public void ToMetadataLoadContext_ShouldReturnMetadataLoadContext()
    {
        // Arrange
        SharpAssemblyResolver.Builder builder = SharpAssemblyResolver
            .CreateBuilder()
            .AddRuntimeEnvironmentAssemblies();
        SharpAssemblyResolver resolver = builder;

        // Act
        var loadContext = resolver.ToMetadataLoadContext();

        // Assert
        Assert.NotNull(loadContext);
        Assert.IsType<MetadataLoadContext>(loadContext);
    }

    [Fact]
    public void LoadAssembly_ShouldLoadAssembly()
    {
        var referenceFiles = new FileInfo[] { new(Assembly.GetExecutingAssembly().Location) };
        var referenceDirectories = new DirectoryInfo[] { new(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)!) };

        using var context = SharpAssemblyResolver.CreateBuilder(outputHelper.ToSharpResolverLogger())
            .AddReferenceFiles(referenceFiles)
            .AddReferenceDirectories(
                new EnumerationOptions()
                {
                    RecurseSubdirectories = true,
                    MaxRecursionDepth = 2,
                },
                referenceDirectories)
            .AddRuntimeEnvironmentAssemblies()
            .ToAssemblyResolver()
            .ToMetadataLoadContext();

        Assembly assembly = context.LoadAssembly(new FileInfo(Assembly.GetExecutingAssembly().Location));
        Assert.NotNull(assembly);
        Assert.Equal(Assembly.GetExecutingAssembly().FullName, assembly.FullName);
    }
}