using System.Reflection;

namespace SharpMeta;

/// <summary>
/// Extension methods for <see cref="PropertyInfo"/>.
/// </summary>
public static class PropertyInfoExtensions
{
    private static readonly ThreadLocal<NullabilityInfoContext> NullabilityContext = new(() => new NullabilityInfoContext());

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

        NullabilityInfo nullabilityInfo = NullabilityContext.Value?.Create(property)
            ?? throw new InvalidOperationException($"Failed creating nullability context for property {property.Name}.");

        return nullabilityInfo.ReadState == NullabilityState.Nullable;
    }

    /// <summary>
    /// Determines whether the specified property is marked with the <see cref="System.Runtime.CompilerServices.RequiredMemberAttribute"/>.
    /// </summary>
    /// <param name="property">The property to check.</param>
    /// <returns><see langword="true"/> if the property is marked with the <see cref="System.Runtime.CompilerServices.RequiredMemberAttribute"/>; otherwise, <see langword="false"/>.</returns>
    public static bool IsRequiredMember(this PropertyInfo property)
    {
        ArgumentNullException.ThrowIfNull(property);

        if (property.TryGetCustomAttributeData<System.Runtime.CompilerServices.RequiredMemberAttribute>(out _))
        {
            return true;
        }

        return false;
    }
}
