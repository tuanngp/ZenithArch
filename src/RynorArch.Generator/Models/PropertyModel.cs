using System;

namespace RynorArch.Generator.Models;

/// <summary>
/// Represents a single property extracted from a domain entity.
/// Immutable and value-equal for incremental caching.
/// </summary>
public sealed class PropertyModel : IEquatable<PropertyModel>
{
    public string Name { get; }
    public string TypeName { get; }
    public bool IsNullable { get; }
    public bool IsCollection { get; }
    public string? CollectionElementType { get; }

    public PropertyModel(
        string name,
        string typeName,
        bool isNullable,
        bool isCollection,
        string? collectionElementType)
    {
        Name = name;
        TypeName = typeName;
        IsNullable = isNullable;
        IsCollection = isCollection;
        CollectionElementType = collectionElementType;
    }

    public bool Equals(PropertyModel? other)
    {
        if (other is null) return false;
        if (ReferenceEquals(this, other)) return true;

        return string.Equals(Name, other.Name, StringComparison.Ordinal)
            && string.Equals(TypeName, other.TypeName, StringComparison.Ordinal)
            && IsNullable == other.IsNullable
            && IsCollection == other.IsCollection
            && string.Equals(CollectionElementType, other.CollectionElementType, StringComparison.Ordinal);
    }

    public override bool Equals(object? obj) => Equals(obj as PropertyModel);

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
            return hash;
        }
    }
}
