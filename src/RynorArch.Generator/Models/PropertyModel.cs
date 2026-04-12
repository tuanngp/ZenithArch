using System;

namespace RynorArch.Generator.Models;

/// <summary>
/// Represents a single property extracted from a domain entity.
/// Immutable and value-equal for incremental caching.
/// </summary>
public sealed class PropertyModel : IEquatable<PropertyModel>
{
    /// <summary>
    /// Gets the property name.
    /// </summary>
    public string Name { get; }

    /// <summary>
    /// Gets the fully formatted property type name.
    /// </summary>
    public string TypeName { get; }

    /// <summary>
    /// Gets a value indicating whether the property type is nullable.
    /// </summary>
    public bool IsNullable { get; }

    /// <summary>
    /// Gets a value indicating whether the property is a collection.
    /// </summary>
    public bool IsCollection { get; }

    /// <summary>
    /// Gets the collection element type name when the property is a collection.
    /// </summary>
    public string? CollectionElementType { get; }

    /// <summary>
    /// Gets discovered validation rules for the property, when present.
    /// </summary>
    public ValidationRules? Validation { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="PropertyModel"/> class.
    /// </summary>
    /// <param name="name">The property name.</param>
    /// <param name="typeName">The formatted property type name.</param>
    /// <param name="isNullable">Whether the property type is nullable.</param>
    /// <param name="isCollection">Whether the property is a collection.</param>
    /// <param name="collectionElementType">The collection element type name when available.</param>
    /// <param name="validation">The discovered validation rule set when available.</param>
    /// <example>
    /// <code>var property = new PropertyModel("Name", "string", false, false, null, rules);</code>
    /// </example>
    public PropertyModel(
        string name,
        string typeName,
        bool isNullable,
        bool isCollection,
        string? collectionElementType,
        ValidationRules? validation)
    {
        Name = name;
        TypeName = typeName;
        IsNullable = isNullable;
        IsCollection = isCollection;
        CollectionElementType = collectionElementType;
        Validation = validation;
    }

    /// <summary>
    /// Determines whether this instance is equal to another property model.
    /// </summary>
    /// <param name="other">The property model to compare.</param>
    /// <returns><see langword="true"/> when all value-equal fields match; otherwise <see langword="false"/>.</returns>
    public bool Equals(PropertyModel? other)
    {
        if (other is null) return false;
        if (ReferenceEquals(this, other)) return true;

        return string.Equals(Name, other.Name, StringComparison.Ordinal)
            && string.Equals(TypeName, other.TypeName, StringComparison.Ordinal)
            && IsNullable == other.IsNullable
            && IsCollection == other.IsCollection
            && string.Equals(CollectionElementType, other.CollectionElementType, StringComparison.Ordinal)
            && Equals(Validation, other.Validation);
    }

    /// <summary>
    /// Determines whether this instance is equal to another object.
    /// </summary>
    /// <param name="obj">The object to compare.</param>
    /// <returns><see langword="true"/> when the object is a matching property model; otherwise <see langword="false"/>.</returns>
    public override bool Equals(object? obj) => Equals(obj as PropertyModel);

    /// <summary>
    /// Computes a stable hash code for incremental cache invalidation.
    /// </summary>
    /// <returns>The hash code for this property model.</returns>
    public override int GetHashCode()
    {
        unchecked
        {
            int hash = (int)2166136261;
            hash = (hash ^ (Name?.GetHashCode() ?? 0)) * 16777619;
            hash = (hash ^ (TypeName?.GetHashCode() ?? 0)) * 16777619;
            hash = (hash ^ IsNullable.GetHashCode()) * 16777619;
            hash = (hash ^ IsCollection.GetHashCode()) * 16777619;
            hash = (hash ^ (CollectionElementType?.GetHashCode() ?? 0)) * 16777619;
            hash = (hash ^ (Validation?.GetHashCode() ?? 0)) * 16777619;
            return hash;
        }
    }
}
