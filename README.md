# SharpMeta

***An opiniated library for inspecting .NET assembly Metadata***

[![CI](https://github.com/trippwill/sharp-meta/actions/workflows/libanvl-dotnet-ci.yml/badge.svg)](https://github.com/trippwill/sharp-meta/actions/workflows/libanvl-dotnet-ci.yml)
[![NuGet package](https://img.shields.io/nuget/v/SharpMeta.svg)](https://nuget.org/packages/SharpMeta)
[![codecov](https://codecov.io/gh/trippwill/sharp-meta/graph/badge.svg?token=uzEl9z9BoS)](https://codecov.io/gh/trippwill/sharp-meta)
[![CodeQL](https://github.com/trippwill/sharp-meta/actions/workflows/github-code-scanning/codeql/badge.svg)](https://github.com/trippwill/sharp-meta/actions/workflows/github-code-scanning/codeql)

# SharpMeta

SharpMeta is a .NET library that facilitates the loading and inspection of .NET assemblies using the `System.Reflection.MetadataLoadContext`.
This library provides a robust and flexible way to load assemblies from specified file paths and directories,
while also providing extension methods for inspecting type and attribute metadata.

Using `SharpAssemblyResolver.Builder.AddReferenceDirectories("path/to/target-framework")`, SharpMeta can load and inspect .NET assemblies compiled for any target framework -- including .NET and .NET Framework.

## Features

- Load assemblies from specified file paths.
- Recursively search directories for assemblies.
- Log errors and information during the loading process.
- Utilize `MetadataLoadContext` for inspecting assemblies without loading them into the main application domain.
- Extension methods for `Type`, `PropertyInfo`, `MemberInfo`, and `CustomAttributeData` to facilitate inspecting type and attribute metadata.

## Getting Started

### Prerequisites

- .NET 8.0 SDK

### Installation

To use SharpMeta in your project, add the following package reference to your `.csproj` file:

```csharp
<ItemGroup>
  <PackageReference Include="SharpMeta" />
</ItemGroup>
```

### Usage

Below is an example of how to use the `SharpAssemblyResolver` class provided by SharpMeta:


```csharp
using System;
using System.IO;
using System.Reflection;
using SharpMeta;

class Program
{
    static void Main()
    {
        var referenceFiles = new FileInfo[]
        {
            new FileInfo("path/to/your/assembly1.dll"),
            new FileInfo("path/to/your/assembly2.dll")
        };

        var referenceDirectories = new DirectoryInfo[]
        {
            new DirectoryInfo("path/to/your/reference/directory")
        };

        using var context = SharpAssemblyResolver.CreateBuilder()
            .AddReferenceFiles(referenceFiles)
            .AddReferenceDirectories(referenceDirectories)
            .ToAssemblyResolver()
            .ToMetadataLoadContext();

        var assembly = context.LoadFromAssemblyPath("path/to/your/target/assembly.dll");
        Console.WriteLine($"Loaded Assembly: {assembly.FullName}");

        // Example usage of extension methods
        foreach (var type in assembly.GetTypes())
        {
            if (type.ImplementsAnyInterface(("System.Collections", "IEnumerable")))
            {
                Console.WriteLine($"{type.FullName} implements IEnumerable");
            }

            if (type.IsProbableDictionary(out var keyType, out var valueType))
            {
                Console.WriteLine($"{type.FullName} is a probable dictionary with key type {keyType} and value type {valueType}");
            }

            foreach (var property in type.GetProperties())
            {
                if (property.IsNullable())
                {
                    Console.WriteLine($"{property.Name} is nullable");
                }
            }

            foreach (var member in type.GetMembers())
            {
                if (member.TryGetCustomAttributeData<ObsoleteAttribute>(out var attributeData))
                {
                    Console.WriteLine($"{member.Name} has ObsoleteAttribute with message: {attributeData.GetNamedArgument<string>("Message")}");
                }
            }
        }
    }
}
```

## Contributing

Contributions are welcome! Please feel free to submit a pull request or open an issue to discuss any changes. See [CONTRIBUTING](CONTRIBUTING.md).

## License

This project is licensed under the MIT License. See the [LICENSE](LICENSE) file for details.

## Acknowledgments

- [System.Reflection.MetadataLoadContext](https://www.nuget.org/packages/System.Reflection.MetadataLoadContext/)
- [Nerdbank.GitVersioning](https://www.nuget.org/packages/Nerdbank.GitVersioning/)
