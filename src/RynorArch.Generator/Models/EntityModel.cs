using System;

namespace RynorArch.Generator.Models;

/// <summary>
/// Intermediate representation of a domain entity extracted from semantic analysis.
/// All fields use value-equal types to ensure incremental caching works correctly.
/// </summary>
public sealed class EntityModel : IEquatable<EntityModel>
{
    public string Name { get; }
    public string Namespace { get; }
    public EquatableArray<PropertyModel> Properties { get; }
    public EquatableArray<PropertyModel> FilterProperties { get; }
    public bool IsAggregateRoot { get; }
    public string? MapToTypeName { get; }
    public string? MapToTypeNamespace { get; }

    public EntityModel(
        string name,
        string @namespace,
        EquatableArray<PropertyModel> properties,
        EquatableArray<PropertyModel> filterProperties,
        bool isAggregateRoot,
        string? mapToTypeName,
        string? mapToTypeNamespace)
    {
        Name = name;
        Namespace = @namespace;
        Properties = properties;
        FilterProperties = filterProperties;
        IsAggregateRoot = isAggregateRoot;
        MapToTypeName = mapToTypeName;
        MapToTypeNamespace = mapToTypeNamespace;
    }

    public bool Equals(EntityModel? other)
    {
        if (other is null) return false;
        if (ReferenceEquals(this, other)) return true;

        return string.Equals(Name, other.Name, StringComparison.Ordinal)
            && string.Equals(Namespace, other.Namespace, StringComparison.Ordinal)
            && Properties == other.Properties
            && FilterProperties == other.FilterProperties
            && IsAggregateRoot == other.IsAggregateRoot
            && string.Equals(MapToTypeName, other.MapToTypeName, StringComparison.Ordinal)
            && string.Equals(MapToTypeNamespace, other.MapToTypeNamespace, StringComparison.Ordinal);
    }

    public override bool Equals(object? obj) => Equals(obj as EntityModel);

    public override int GetHashCode()
    {
        unchecked
        {
            int hash = (int)2166136261;
            hash = (hash ^ (Name?.GetHashCode() ?? 0)) * 16777619;
            hash = (hash ^ (Namespace?.GetHashCode() ?? 0)) * 16777619;
            hash = (hash ^ Properties.GetHashCode()) * 16777619;
            hash = (hash ^ FilterProperties.GetHashCode()) * 16777619;
            hash = (hash ^ IsAggregateRoot.GetHashCode()) * 16777619;
            hash = (hash ^ (MapToTypeName?.GetHashCode() ?? 0)) * 16777619;
            hash = (hash ^ (MapToTypeNamespace?.GetHashCode() ?? 0)) * 16777619;
            return hash;
        }
    }
}
