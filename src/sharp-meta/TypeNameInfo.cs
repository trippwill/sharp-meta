namespace SharpMeta;

/// <summary>
/// Specifies the style of the type or member name.
/// </summary>
public enum NameStyle
{
    /// <summary>
    /// The default style.
    /// </summary>
    Framework,

    /// <summary>
    /// The style used for generating XML documentation IDs.
    /// </summary>
    DocId,

    /// <summary>
    /// The style used for generating parameter types in XML documentation IDs.
    /// </summary>
    DocIdParameter,
}

/// <summary>
/// Represents type name information.
/// </summary>
/// <param name="ShortName">The short name of the type.</param>
public abstract record TypeNameInfo(string ShortName)
{
    /// <summary>
    /// Creates a <see cref="TypeNameInfo"/> from a <see cref="Type"/>.
    /// </summary>
    /// <param name="type">The type to create from.</param>
    /// <returns>A <see cref="TypeNameInfo"/> representing the type.</returns>
    /// <exception cref="ArgumentNullException">Thrown when the type is null.</exception>
    public static TypeNameInfo From(Type type)
    {
        ArgumentNullException.ThrowIfNull(type);

        if (type.IsGenericType)
        {
            if (type.Name.StartsWith("<>f__AnonymousType"))
                return new Dynamic(type.Name);

            TypeNameInfo[] genericArguments = [.. type.GetGenericArguments().Select(From)];
            var arity = genericArguments.Length;

            return type.IsGenericTypeDefinition
                ? new OpenGeneric(VerifyFullName(type.FullName), type.Name, arity, genericArguments)
                : type.FullName is null
                    ? new ClosedGeneric(VerifyFullName(type.GetGenericTypeDefinition().FullName), type.Name, arity, genericArguments)
                    : new ClosedGeneric(VerifyFullName(type.FullName), type.Name, arity, genericArguments);
        }
        else if (type.IsArray)
        {
            return new Array(VerifyFullName(type.FullName), type.Name, From(type.GetElementType()!), type.GetArrayRank());
        }
        else if (type.IsByRef)
        {
            return new ByRef(VerifyFullName(type.FullName), type.Name, From(type.GetElementType()!));
        }
        else if (type.IsPointer)
        {
            return new Pointer(VerifyFullName(type.FullName), type.Name, From(type.GetElementType()!));
        }
        else if (type.IsGenericParameter)
        {
            return new GenericParameter(type.Name, type.GenericParameterPosition);
        }

        return new Standard(VerifyFullName(type.FullName), type.Name);
    }

    /// <summary>
    /// Converts a <see cref="Type"/> to a <see cref="TypeNameInfo"/>.
    /// </summary>
    /// <param name="type">The type to convert.</param>
    public static implicit operator TypeNameInfo(Type type) => From(type);

    /// <summary>
    /// Normalizes the full name based on the specified style.
    /// </summary>
    /// <param name="style">The style to use for normalization.</param>
    /// <param name="fullName">The full name of the type.</param>
    /// <returns>The normalized full name.</returns>
    /// <exception cref="NotSupportedException">Thrown when the style is not supported.</exception>
    private static string NormalizeFullName(NameStyle style, string fullName)
    {
        return style switch
        {
            NameStyle.Framework => fullName,
            NameStyle.DocId or NameStyle.DocIdParameter => fullName.Replace('+', '.'),
            _ => throw new NotSupportedException($"Style not supported: {style}")
        };
    }

    /// <summary>
    /// Strips the generic arguments from the type name.
    /// </summary>
    /// <param name="name">The name of the type.</param>
    /// <returns>The name without generic arguments.</returns>
    private static string StripGenericArguments(string name)
    {
        int index = name.IndexOf('`');
        return index == -1 ? name : name[..index];
    }

    /// <summary>
    /// Verifies that the value is not null.
    /// </summary>
    /// <param name="value">The value to verify.</param>
    /// <returns>The verified value.</returns>
    /// <exception cref="InvalidOperationException">Thrown when the value is null.</exception>
    private static string VerifyFullName(string? value) => value ?? throw new InvalidOperationException("FullName cannot be null.");

    /// <summary>
    /// Gets the short name based on the specified style.
    /// </summary>
    /// <param name="style">The style to use.</param>
    /// <returns>The short name.</returns>
    public string GetShortName(NameStyle style) => this.GetShortNameCore(style);

    /// <summary>
    /// Gets the full name based on the specified style.
    /// </summary>
    /// <param name="style">The style to use.</param>
    /// <returns>The full name.</returns>
    public string GetFullName(NameStyle style) => this.GetFullNameCore(style);

    /// <summary>
    /// Gets the short name based on the specified style.
    /// </summary>
    /// <param name="style">The style to use.</param>
    /// <returns>The short name.</returns>
    protected abstract string GetShortNameCore(NameStyle style);

    /// <summary>
    /// Gets the full name based on the specified style.
    /// </summary>
    /// <param name="style">The style to use.</param>
    /// <returns>The full name.</returns>
    protected abstract string GetFullNameCore(NameStyle style);

    /// <summary>
    /// Represents a standard type name.
    /// </summary>
    /// <param name="FullName">The full name of the type.</param>
    /// <param name="ShortName">The short name of the type.</param>
    public record Standard(string FullName, string ShortName) : TypeNameInfo(ShortName)
    {
        /// <inheritdoc />
        protected override string GetShortNameCore(NameStyle style) => this.ShortName;

        /// <inheritdoc />
        protected override string GetFullNameCore(NameStyle style) => NormalizeFullName(style, this.FullName);
    }

    /// <summary>
    /// Represents an open generic type name.
    /// </summary>
    /// <param name="FullName">The full name of the type.</param>
    /// <param name="ShortName">The short name of the type.</param>
    /// <param name="Arity">The number of generic arguments.</param>
    /// <param name="TypeArguments">The type arguments.</param>
    public record OpenGeneric(string FullName, string ShortName, int Arity, TypeNameInfo[] TypeArguments) : TypeNameInfo(ShortName)
    {
        /// <inheritdoc />
        protected override string GetShortNameCore(NameStyle style) => style switch
        {
            NameStyle.Framework or NameStyle.DocId => this.ShortName,
            NameStyle.DocIdParameter => $"{StripGenericArguments(this.ShortName)}{{{string.Join(",", this.TypeArguments.Select(t => t.GetFullName(NameStyle.DocId)))}}}",
            _ => throw new NotSupportedException($"Style not supported: {style}")
        };

        /// <inheritdoc />
        protected override string GetFullNameCore(NameStyle style) => style switch
        {
            NameStyle.Framework => this.FullName,
            NameStyle.DocId => $"{NormalizeFullName(style, StripGenericArguments(this.FullName))}`{this.Arity}",
            NameStyle.DocIdParameter => $"{NormalizeFullName(style, StripGenericArguments(this.FullName))}{{{string.Join(",", this.TypeArguments.Select(t => t.GetFullName(NameStyle.DocId)))}}}",
            _ => throw new NotSupportedException($"Style not supported: {style}")
        };
    }

    /// <summary>
    /// Represents a closed generic type name.
    /// </summary>
    /// <param name="FullName">The full name of the type.</param>
    /// <param name="ShortName">The short name of the type.</param>
    /// <param name="Arity">The number of generic arguments.</param>
    /// <param name="TypeArguments">The type arguments.</param>
    public record ClosedGeneric(string FullName, string ShortName, int Arity, TypeNameInfo[] TypeArguments) : TypeNameInfo(ShortName)
    {
        /// <inheritdoc />
        protected override string GetShortNameCore(NameStyle style) => style switch
        {
            NameStyle.Framework or NameStyle.DocId => this.ShortName,
            NameStyle.DocIdParameter => $"{StripGenericArguments(this.ShortName)}{{{string.Join(",", this.TypeArguments.Select(t => t.GetFullName(NameStyle.DocId)))}}}",
            _ => throw new NotSupportedException($"Style not supported: {style}")
        };

        /// <inheritdoc />
        protected override string GetFullNameCore(NameStyle style) => style switch
        {
            NameStyle.Framework => this.FullName,
            NameStyle.DocId => $"{NormalizeFullName(style, StripGenericArguments(this.FullName))}`{this.Arity}",
            NameStyle.DocIdParameter => $"{NormalizeFullName(style, StripGenericArguments(this.FullName))}{{{string.Join(",", this.TypeArguments.Select(t => t.GetFullName(NameStyle.DocId)))}}}",
            _ => throw new NotSupportedException($"Style not supported: {style}")
        };
    }

    /// <summary>
    /// Represents an array type name.
    /// </summary>
    /// <param name="FullName">The full name of the type.</param>
    /// <param name="ShortName">The short name of the type.</param>
    /// <param name="ElementType">The element type of the array.</param>
    /// <param name="Rank">The rank of the array.</param>
    public record Array(string FullName, string ShortName, TypeNameInfo ElementType, int Rank) : TypeNameInfo(ShortName)
    {
        /// <inheritdoc />
        protected override string GetShortNameCore(NameStyle style) => style switch
        {
            NameStyle.Framework => this.ShortName,
            NameStyle.DocId or NameStyle.DocIdParameter => $"{this.ElementType.GetShortName(style)}[{GetArrayDimensions(this.Rank)}]",
            _ => throw new NotSupportedException($"Style not supported: {style}")
        };

        /// <inheritdoc />
        protected override string GetFullNameCore(NameStyle style) => style switch
        {
            NameStyle.Framework => this.FullName,
            NameStyle.DocId or NameStyle.DocIdParameter => $"{this.ElementType.GetFullName(style)}[{GetArrayDimensions(this.Rank)}]",
            _ => throw new NotSupportedException($"Style not supported: {style}")
        };

        private static string GetArrayDimensions(int rank)
        {
            if (rank == 1)
            {
                return string.Empty;
            }

            var dimensions = new List<string>();
            for (int i = 0; i < rank; i++)
            {
                dimensions.Add("0:");
            }

            return string.Join(",", dimensions);
        }
    }

    /// <summary>
    /// Represents a pointer type name.
    /// </summary>
    /// <param name="FullName">The full name of the type.</param>
    /// <param name="ShortName">The short name of the type.</param>
    /// <param name="ElementType">The element type of the pointer.</param>
    public record Pointer(string FullName, string ShortName, TypeNameInfo ElementType) : TypeNameInfo(ShortName)
    {
        /// <inheritdoc />
        protected override string GetShortNameCore(NameStyle style) => style switch
        {
            NameStyle.Framework => this.ShortName,
            NameStyle.DocId or NameStyle.DocIdParameter => $"{this.ElementType.GetShortName(style)}*",
            _ => throw new NotSupportedException($"Style not supported: {style}")
        };

        /// <inheritdoc />
        protected override string GetFullNameCore(NameStyle style) => style switch
        {
            NameStyle.Framework => this.FullName,
            NameStyle.DocId or NameStyle.DocIdParameter => $"{this.ElementType.GetFullName(style)}*",
            _ => throw new NotSupportedException($"Style not supported: {style}")
        };
    }

    /// <summary>
    /// Represents a by-reference type name.
    /// </summary>
    /// <param name="FullName">The full name of the type.</param>
    /// <param name="ShortName">The short name of the type.</param>
    /// <param name="ElementType">The element type of the by-reference.</param>
    public record ByRef(string FullName, string ShortName, TypeNameInfo ElementType) : TypeNameInfo(ShortName)
    {
        /// <inheritdoc />
        protected override string GetShortNameCore(NameStyle style) => style switch
        {
            NameStyle.Framework => this.ShortName,
            NameStyle.DocId or NameStyle.DocIdParameter => $"{this.ElementType.GetShortName(style)}@",
            _ => throw new NotSupportedException($"Style not supported: {style}")
        };

        /// <inheritdoc />
        protected override string GetFullNameCore(NameStyle style) => style switch
        {
            NameStyle.Framework => this.FullName,
            NameStyle.DocId or NameStyle.DocIdParameter => $"{this.ElementType.GetFullName(style)}@",
            _ => throw new NotSupportedException($"Style not supported: {style}")
        };
    }

    /// <summary>
    /// Represents a generic parameter type name.
    /// </summary>
    /// <param name="ShortName">The type name of the generic parameter.</param>
    /// <param name="Position">The position of the generic parameter.</param>
    public record GenericParameter(string ShortName, int Position) : TypeNameInfo(ShortName)
    {
        /// <inheritdoc />
        protected override string GetShortNameCore(NameStyle style) => style
            switch
        {
            NameStyle.Framework => this.ShortName,
            NameStyle.DocId or NameStyle.DocIdParameter => $"``{this.Position}",
            _ => throw new NotSupportedException($"Style not supported: {style}")
        };

        /// <inheritdoc />
        protected override string GetFullNameCore(NameStyle style) => this.GetShortNameCore(style);
    }

    /// <summary>
    /// Represents a dynamic type name.
    /// </summary>
    /// <param name="ShortName">The short name of the dynamic type.</param>
    public record Dynamic(string ShortName) : TypeNameInfo(ShortName)
    {
        /// <inheritdoc />
        protected override string GetShortNameCore(NameStyle style) => this.ShortName;

        /// <inheritdoc />
        protected override string GetFullNameCore(NameStyle style) => this.ShortName;
    }
}
