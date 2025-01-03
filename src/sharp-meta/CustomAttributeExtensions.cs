using System.Diagnostics.CodeAnalysis;
using System.Reflection;

namespace SharpMeta;

/// <summary>
/// Provides extension methods for <see cref="CustomAttributeData"/> instances.
/// </summary>
public static class CustomAttributeDataExtensions
{
    /// <summary>
    /// Retrieves the value of a named argument from the specified <see cref="CustomAttributeData"/> instance.
    /// </summary>
    /// <typeparam name="T">The type of the argument value.</typeparam>
    /// <param name="customAttributeData">The <see cref="CustomAttributeData"/> instance.</param>
    /// <param name="name">The name of the argument.</param>
    /// <returns>The value of the named argument, or the default value of type <typeparamref name="T"/> if the argument is not found.</returns>
    public static T? GetNamedArgument<T>(this CustomAttributeData customAttributeData, string name)
    {
        ArgumentNullException.ThrowIfNull(customAttributeData);

        return customAttributeData
            .NamedArguments
            .FirstOrDefault(a => a.MemberName == name)
            .TypedValue.Value is T value ? value : default;
    }

    /// <summary>
    /// Tries to retrieve the value of a named argument from the specified <see cref="CustomAttributeData"/> instance.
    /// </summary>
    /// <typeparam name="T">The type of the argument value.</typeparam>
    /// <param name="customAttributeData">The <see cref="CustomAttributeData"/> instance.</param>
    /// <param name="name">The name of the argument.</param>
    /// <param name="value">When this method returns, contains the value of the named argument, if found; otherwise, the default value of type <typeparamref name="T"/>.</param>
    /// <returns><see langword="true"/> if the named argument is found and its value is successfully retrieved; otherwise, <see langword="false"/>.</returns>
    public static bool TryGetNamedArgument<T>(this CustomAttributeData customAttributeData, string name, [NotNullWhen(true)] out T? value)
    {
        ArgumentNullException.ThrowIfNull(customAttributeData);

        CustomAttributeNamedArgument? argument = customAttributeData
            .NamedArguments
            .FirstOrDefault(a => a.MemberName == name);

        if (argument.Value is CustomAttributeNamedArgument namedArgument &&
            namedArgument.TypedValue.Value is object presentValue)
        {
            value = (T)presentValue;
            return true;
        }

        value = default;
        return false;
    }

    /// <summary>
    /// Retrieves the value of a constructor argument from the specified <see cref="CustomAttributeData"/> instance.
    /// </summary>
    /// <typeparam name="T">The type of the argument value.</typeparam>
    /// <param name="customAttributeData">The <see cref="CustomAttributeData"/> instance.</param>
    /// <param name="index">The index of the constructor argument.</param>
    /// <returns>The value of the constructor argument, or the default value of type <typeparamref name="T"/> if the argument is not found.</returns>
    public static T? GetConstructorArgument<T>(this CustomAttributeData customAttributeData, int index)
    {
        ArgumentNullException.ThrowIfNull(customAttributeData);

        return customAttributeData
            .ConstructorArguments
            .ElementAtOrDefault(index)
            .Value is T value ? value : default;
    }

    /// <summary>
    /// Tries to retrieve the value of a constructor argument from the specified <see cref="CustomAttributeData"/> instance.
    /// </summary>
    /// <typeparam name="T">The type of the argument value.</typeparam>
    /// <param name="customAttributeData">The <see cref="CustomAttributeData"/> instance.</param>
    /// <param name="index">The index of the constructor argument.</param>
    /// <param name="value">When this method returns, contains the value of the constructor argument, if found; otherwise, the default value of type <typeparamref name="T"/>.</param>
    /// <returns><see langword="true"/> if the constructor argument is found and its value is successfully retrieved; otherwise, <see langword="false"/>.</returns>
    public static bool TryGetConstructorArgument<T>(this CustomAttributeData customAttributeData, int index, [NotNullWhen(true)] out T? value)
    {
        ArgumentNullException.ThrowIfNull(customAttributeData);

        CustomAttributeTypedArgument? argument = customAttributeData
            .ConstructorArguments
            .ElementAtOrDefault(index);

        if (argument.Value is CustomAttributeTypedArgument typedArgument &&
            typedArgument.Value is object presentValue)
        {
            value = (T)presentValue;
            return true;
        }

        value = default;
        return false;
    }
}
