# SharpMeta

***An opiniated library for inspecting .NET assembly Metadata***

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

### API Reference

#### `SharpAssemblyResolver`

- **Methods**
  - `public static Builder CreateBuilder(SharpResolverLogger? logger = null)`: Creates a new `Builder` instance.
  - `public static MetadataLoadContext CreateExecutingAssemblyLoadContext()`: Creates a `MetadataLoadContext` for the executing assembly.
  - `public static implicit operator SharpAssemblyResolver(Builder builder)`: Implicitly converts a `Builder` to a `SharpAssemblyResolver`.
  - `public static implicit operator MetadataLoadContext(SharpAssemblyResolver resolver)`: Implicitly converts a `SharpAssemblyResolver` to a `MetadataLoadContext`.
  - `public MetadataLoadContext ToMetadataLoadContext()`: Converts the resolver to a `MetadataLoadContext`.

#### `SharpAssemblyResolver.Builder`

- **Constructor**


```csharp
internal Builder(SharpResolverLogger? logger = null)
```

  - `logger`: The logger to use for load context events.

- **Methods**
  - `public SharpAssemblyResolver ToAssemblyResolver()`: Converts the builder to a `SharpAssemblyResolver`.
  - `public Builder AddReferenceDirectory(DirectoryInfo directory, EnumerationOptions? options = null)`: Adds a reference directory to the builder.
  - `public Builder AddReferenceDirectories(EnumerationOptions options, params DirectoryInfo[] directories)`: Adds multiple reference directories to the builder.
  - `public Builder AddReferenceFile(FileInfo file, bool includeSideBySideAssemblies = false)`: Adds a reference file to the builder.
  - `public Builder AddReferenceFiles(params FileInfo[] files)`: Adds multiple reference files to the builder.
  - `public Builder AddExecutingAssembly(bool includeSideBySideAssemblies = false, int sideBySideRecursionDepth = 1)`: Adds the executing assembly to the builder.
  - `public Builder AddRuntimeEnvironmentAssemblies()`: Adds the runtime assemblies of the executing application to the builder.

### Extension Methods

#### `TypeExtensions`

- **`ImplementsAnyInterface`**


```csharp
public static bool ImplementsAnyInterface(this Type type, params (string? Namespace, string Name)[] interfaceNames)
```

  Determines whether the specified `Type` implements any of the specified interface names.

- **`TryUnwrapNullable`**


```csharp
public static bool TryUnwrapNullable(this Type type, [NotNullWhen(true)] out Type? underlyingType)
```

  Tries to unwrap a nullable type and get its underlying type.

- **`IsProbableDictionary`**


```csharp
public static bool IsProbableDictionary(this Type type, [NotNullWhen(true)] out Type? keyType, [NotNullWhen(true)] out Type? valueType)
```

  Determines whether the specified `Type` is a probable dictionary.

#### `PropertyInfoExtensions`

- **`IsNullable`**


```csharp
public static bool IsNullable(this PropertyInfo property)
```

  Determines whether the specified property is nullable.

- **`IsNullableReference`**


```csharp
public static bool IsNullableReference(this PropertyInfo property)
```

  Determines whether the specified property is a nullable reference type.

#### `MemberInfoExtensions`

- **`TryGetCustomAttributeData<T>`**


```csharp
public static bool TryGetCustomAttributeData<T>(this IList<CustomAttributeData>? attributeDatas, [NotNullWhen(true)] out CustomAttributeData? attributeData)
    where T : Attribute
```

  Tries to get the `CustomAttributeData` from the attribute data collection and attribute type.

- **`TryGetCustomAttributeData<T>`**


```csharp
public static bool TryGetCustomAttributeData<T>(this MemberInfo memberInfo, [NotNullWhen(true)] out CustomAttributeData? attributeData)
    where T : Attribute
```

  Tries to get the `CustomAttributeData` for the specified `MemberInfo` and attribute type.

- **`TryGetCustomAttributeData`**


```csharp
public static bool TryGetCustomAttributeData(this MemberInfo memberInfo, Type attributeType, [NotNullWhen(true)] out CustomAttributeData? attributeData)
```

  Tries to get the `CustomAttributeData` for the specified `MemberInfo` and attribute type.

- **`TryGetCustomAttributeData`**


```csharp
public static bool TryGetCustomAttributeData(this MemberInfo memberInfo, string attributeFullName, [NotNullWhen(true)] out CustomAttributeData? attributeData)
```

  Tries to get the `CustomAttributeData` for the specified `Type` and attribute type.

#### `CustomAttributeDataExtensions`

- **`GetNamedArgument<T>`**


```csharp
public static T? GetNamedArgument<T>(this CustomAttributeData customAttributeData, string name)
```

  Retrieves the value of a named argument from the specified `CustomAttributeData` instance.

- **`TryGetNamedArgument<T>`**


```csharp
public static bool TryGetNamedArgument<T>(this CustomAttributeData customAttributeData, string name, [NotNullWhen(true)] out T? value)
```

  Tries to retrieve the value of a named argument from the specified `CustomAttributeData` instance.

- **`GetConstructorArgument<T>`**


```csharp
public static T? GetConstructorArgument<T>(this CustomAttributeData customAttributeData, int index)
```

  Retrieves the value of a constructor argument from the specified `CustomAttributeData` instance.

- **`TryGetConstructorArgument<T>`**


```csharp
public static bool TryGetConstructorArgument<T>(this CustomAttributeData customAttributeData, int index, [NotNullWhen(true)] out T? value)
```

  Tries to retrieve the value of a constructor argument from the specified `CustomAttributeData` instance.

## Contributing

Contributions are welcome! Please feel free to submit a pull request or open an issue to discuss any changes. See [CONTRIBUTING](CONTRIBUTING.md).

## License

This project is licensed under the MIT License. See the [LICENSE](LICENSE) file for details.

## Acknowledgments

- [System.Reflection.MetadataLoadContext](https://www.nuget.org/packages/System.Reflection.MetadataLoadContext/)
- [Nerdbank.GitVersioning](https://www.nuget.org/packages/Nerdbank.GitVersioning/)
