using System.Reflection;

namespace SharpMeta;

/// <summary>
/// Extension methods for <see cref="PropertyInfo"/>.
/// </summary>
public static class PropertyInfoExtensions
{
    private static readonly ThreadLocal<NullabilityInfoContext> ThreadLocalNullabilityContext = new(() => new NullabilityInfoContext());

    /// <summary>
    /// Determines whether the specified property is nullable.
    /// </summary>
    /// <param name="property">The property to check.</param>
    /// <returns><see langword="true"/> if the property is nullable; otherwise, <see langword="false"/>.</returns>
    public static bool IsNullable(this PropertyInfo property)
    {
        ArgumentNullException.ThrowIfNull(property);

        if (property.PropertyType.IsValueType &&
            property.PropertyType.IsGenericType &&
            property.PropertyType.GetGenericTypeDefinition().Name == typeof(Nullable<>).Name)
        {
            return true;
        }
        else if (property.IsNullableReference())
        {
            return true;
        }

        return false;
    }

    /// <summary>
    /// Determines whether the specified property is a nullable reference type.
    /// </summary>
    /// <param name="property">The property to check.</param>
    /// <returns><c><see langword="true"/></c> if the property is a nullable reference type; otherwise, <see langword="false"/>.</returns>
    public static bool IsNullableReference(this PropertyInfo property)
    {
        ArgumentNullException.ThrowIfNull(property);

        NullabilityInfo? nullabilityInfo = ThreadLocalNullabilityContext.Value?.Create(property);

        return nullabilityInfo is null
            ? throw new InvalidOperationException("Failed to retrieve nullability information.")
            : nullabilityInfo.ReadState == NullabilityState.Nullable;
    }
}
