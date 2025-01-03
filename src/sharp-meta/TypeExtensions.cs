using System.Diagnostics.CodeAnalysis;

namespace SharpMeta;

/// <summary>
/// Provides extension methods for <see cref="Type"/>.
/// </summary>
public static class TypeExtensions
{
    /// <summary>
    /// Determines whether the specified <see cref="Type"/> implements the interface <typeparamref name="T"/>.
    /// </summary>
    /// <typeparam name="T">The interface type to check for.</typeparam>
    /// <param name="type">The <see cref="Type"/> to check.</param>
    /// <returns><see langword="true"/> if the <see cref="Type"/> implements the interface <typeparamref name="T"/>; otherwise, <see langword="false"/>.</returns>
    public static bool ImplementsInterface<T>(this Type type)
    {
        return type.ImplementsAnyInterface(typeof(T).FullName);
    }

    /// <summary>
    /// Determines whether the specified <see cref="Type"/> implements any of the specified interface names.
    /// </summary>
    /// <param name="type">The <see cref="Type"/> to check.</param>
    /// <param name="interfaceNames">The full names of the interfaces to check for.</param>
    /// <returns><see langword="true"/> if the <see cref="Type"/> implements any of the specified interface names; otherwise, <see langword="false"/>.</returns>
    public static bool ImplementsAnyInterface(this Type type, params ReadOnlySpan<string?> interfaceNames)
    {
        ArgumentNullException.ThrowIfNull(type);

        string? typeName = type.FullName;

        if (typeName is not null && interfaceNames.Contains(typeName))
            return true;

        return CheckInterfaces(type, interfaceNames);

        static bool CheckInterfaces(Type? type, ReadOnlySpan<string?> interfaceNames)
        {
            if (type is null)
                return false;

            foreach (Type iface in type.GetInterfaces())
            {
                string? ifaceName = iface.FullName;
                if (ifaceName is not null && interfaceNames.Contains(ifaceName))
                    return true;
            }

            return CheckInterfaces(type.BaseType, interfaceNames);
        }
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
            return false;

        Type? inspectedType = type;

        while (inspectedType is not null)
        {
            if (IsEnumerableOfKeyValuePair(inspectedType, out keyType, out valueType))
                return true;

            inspectedType = inspectedType.BaseType;
        }

        return false;

        static bool IsEnumerableOfKeyValuePair(Type type, [NotNullWhen(true)] out Type? keyType, [NotNullWhen(true)] out Type? valueType)
        {
            keyType = null;
            valueType = null;

            Type[] interfaces = type.GetInterfaces();

            foreach (Type iface in interfaces)
            {
                if (iface.IsGenericType
                    && iface.GetGenericTypeDefinition().Name == typeof(IEnumerable<>).Name
                    && iface.GenericTypeArguments.SingleOrDefault() is Type pairType
                    && pairType.IsGenericType
                    && pairType.GetGenericTypeDefinition().Name == typeof(KeyValuePair<,>).Name)
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

    /// <summary>
    /// Determines whether the specified <see cref="Type"/> is a collection.
    /// </summary>
    /// <param name="type">The <see cref="Type"/> to check.</param>
    /// <returns><see langword="true"/> if the <see cref="Type"/> is a collection; otherwise, <see langword="false"/>.</returns>
    public static bool IsCollection(this Type type)
    {
        ArgumentNullException.ThrowIfNull(type);

        return type.GetInterface(nameof(System.Collections.IEnumerable)) is not null;
    }

    /// <summary>
    /// Determines whether the specified <see cref="Type"/> is a delegate.
    /// </summary>
    /// <param name="type">The <see cref="Type"/> to check.</param>
    /// <returns><see langword="true"/> if the <see cref="Type"/> is a delegate; otherwise, <see langword="false"/>.</returns>
    public static bool IsDelegate(this Type type)
    {
        ArgumentNullException.ThrowIfNull(type);

        return typeof(Delegate).IsAssignableFrom(type);
    }
}
