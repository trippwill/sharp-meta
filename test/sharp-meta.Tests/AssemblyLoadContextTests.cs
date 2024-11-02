using System.Reflection;
using Xunit;
using Xunit.Abstractions;

namespace SharpMeta.Tests;

public class AssemblyLoadContextTests(ITestOutputHelper outputHelper)
{
    [Fact]
    public void Constructor_ShouldInitializeProperties()
    {
        var referenceFiles = new FileInfo[] { new FileInfo(Assembly.GetExecutingAssembly().Location) };
        var referenceDirectories = new DirectoryInfo[] { new DirectoryInfo(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)) };

        using var context = new AssemblyLoadContext(
            logAction: outputHelper.WriteLine,
            directoryRecursionDepth: 2,
            referenceFiles: referenceFiles,
            referenceDirectories: referenceDirectories,
            includeExecutingCoreAssembly: true,
            includeExecutingRuntimeAssemblies: true);

        Assert.NotNull(context.LoadContext);
    }

    [Fact]
    public void LoadAssembly_ShouldLoadAssembly()
    {
        var referenceFiles = new FileInfo[] { new FileInfo(Assembly.GetExecutingAssembly().Location) };
        var referenceDirectories = new DirectoryInfo[] { new DirectoryInfo(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)) };

        using var context = new AssemblyLoadContext(
            logAction: outputHelper.WriteLine,
            directoryRecursionDepth: 2,
            referenceFiles: referenceFiles,
            referenceDirectories: referenceDirectories,
            includeExecutingCoreAssembly: true,
            includeExecutingRuntimeAssemblies: true);

        var assembly = context.LoadAssembly(new FileInfo(Assembly.GetExecutingAssembly().Location));
        Assert.NotNull(assembly);
        Assert.Equal(Assembly.GetExecutingAssembly().FullName, assembly.FullName);
    }
}
