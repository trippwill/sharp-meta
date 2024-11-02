// Copyright (c) Charles Willis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Diagnostics.CodeAnalysis;

namespace SharpMeta;

/// <summary>
/// Provides extension methods for <see cref="Type"/>.
/// </summary>
public static class TypeExtensions
{
    /// <summary>
    /// Determines whether the specified <see cref="Type"/> implements any of the specified interface names.
    /// </summary>
    /// <param name="type">The <see cref="Type"/> to check.</param>
    /// <param name="interfaceNames">The names of the interfaces to check for.</param>
    /// <returns><see langword="true"/> if the <see cref="Type"/> implements any of the specified interface names; otherwise, <see langword="false"/>.</returns>
    public static bool ImplementsAnyInterface(this Type type, params (string? Namespace, string Name)[] interfaceNames)
    {
        ArgumentNullException.ThrowIfNull(type);

        if (interfaceNames.Contains((type.Namespace, type.Name)))
        {
            return true;
        }

        Type? test = type;
        while (test is not null)
        {
            foreach (Type iface in test.GetInterfaces())
            {
                if (interfaceNames.Contains((iface.Namespace, iface.Name)))
                {
                    return true;
                }
            }

            test = test.BaseType;
        }

        return false;
    }

    /// <summary>
    /// Tries to unwrap a nullable type and get its underlying type.
    /// </summary>
    /// <param name="type">The type to unwrap.</param>
    /// <param name="underlyingType">When this method returns, contains the underlying type of the nullable type, if successful; otherwise, <see langword="null"/>.</param>
    /// <returns><see langword="true"/> if the type is a nullable type and the underlying type is successfully obtained; otherwise, <see langword="false"/>.</returns>
    public static bool TryUnwrapNullable(this Type type, [NotNullWhen(true)] out Type? underlyingType)
    {
        ArgumentNullException.ThrowIfNull(type);

        if (type.IsGenericType && type.GetGenericTypeDefinition().Name == typeof(Nullable<>).Name)
        {
            underlyingType = type.GetGenericArguments()[0];
            return true;
        }

        underlyingType = null;
        return false;
    }

    /// <summary>
    /// Determines whether the specified <see cref="Type"/> is a probable dictionary.
    /// </summary>
    /// <param name="type">The <see cref="Type"/> to check.</param>
    /// <param name="keyType">When this method returns, contains the key type of the dictionary, if successful; otherwise, <see langword="null"/>.</param>
    /// <param name="valueType">When this method returns, contains the value type of the dictionary, if successful; otherwise, <see langword="null"/>.</param>
    /// <returns><see langword="true"/> if the type is a probable dictionary; otherwise, <see langword="false"/>.</returns>
    public static bool IsProbableDictionary(this Type type, [NotNullWhen(true)] out Type? keyType, [NotNullWhen(true)] out Type? valueType)
    {
        ArgumentNullException.ThrowIfNull(type);

        keyType = null;
        valueType = null;

        if (!type.IsGenericType)
        {
            return false;
        }

        Type? inspectedType = type;

        while (inspectedType is not null)
        {
            if (IsProbableDictionaryImpl(inspectedType, out keyType, out valueType))
            {
                return true;
            }

            inspectedType = inspectedType.BaseType;
        }

        return false;

        static bool IsProbableDictionaryImpl(Type type, [NotNullWhen(true)] out Type? keyType, [NotNullWhen(true)] out Type? valueType)
        {
            keyType = null;
            valueType = null;

            Type[] interfaces = type.GetInterfaces();

            foreach (Type iface in interfaces)
            {
                if (iface.IsGenericType &&
                    iface.GetGenericTypeDefinition().Name == typeof(IEnumerable<>).Name &&
                    iface.GenericTypeArguments.SingleOrDefault() is Type pairType &&
                    pairType.IsGenericType &&
                    pairType.GetGenericTypeDefinition().Name == typeof(KeyValuePair<,>).Name)
                {
                    Type[] args = pairType.GetGenericArguments();
                    keyType = args[0];
                    valueType = args[1];

                    return true;
                }
            }

            return false;
        }
    }
}
