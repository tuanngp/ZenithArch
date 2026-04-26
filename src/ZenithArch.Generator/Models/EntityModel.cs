using System;

namespace ZenithArch.Generator.Models;

/// <summary>
/// Intermediate representation of a domain entity extracted from semantic analysis.
/// All fields use value-equal types to ensure incremental caching works correctly.
/// </summary>
public sealed class EntityModel : IEquatable<EntityModel>
{
    /// <summary>
    /// Gets the entity type name.
    /// </summary>
    public string Name { get; }

    /// <summary>
    /// Gets the entity namespace.
    /// </summary>
    public string Namespace { get; }

    /// <summary>
    /// Gets all discovered entity properties.
    /// </summary>
    public EquatableArray<PropertyModel> Properties { get; }

    /// <summary>
    /// Gets properties marked for query filter generation.
    /// </summary>
    public EquatableArray<PropertyModel> FilterProperties { get; }

    /// <summary>
    /// Gets a value indicating whether the entity is marked as an aggregate root.
    /// </summary>
    public bool IsAggregateRoot { get; }

    /// <summary>
    /// Gets the mapped target type name when <c>MapTo</c> is configured.
    /// </summary>
    public string? MapToTypeName { get; }

    /// <summary>
    /// Gets the mapped target type namespace when <c>MapTo</c> is configured.
    /// </summary>
    public string? MapToTypeNamespace { get; }

    /// <summary>
    /// Gets a value indicating whether the entity implements soft delete semantics.
    /// </summary>
    public bool IsSoftDelete { get; }

    /// <summary>
    /// Gets a value indicating whether the entity implements audit semantics.
    /// </summary>
    public bool IsAuditable { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="EntityModel"/> class.
    /// </summary>
    /// <param name="name">The entity type name.</param>
    /// <param name="namespace">The entity namespace.</param>
    /// <param name="properties">All discovered entity properties.</param>
    /// <param name="filterProperties">Properties marked for query filter generation.</param>
    /// <param name="isAggregateRoot">Whether the entity is an aggregate root.</param>
    /// <param name="mapToTypeName">Optional mapped type name.</param>
    /// <param name="mapToTypeNamespace">Optional mapped type namespace.</param>
    /// <param name="isSoftDelete">Whether soft-delete behavior is detected.</param>
    /// <param name="isAuditable">Whether audit behavior is detected.</param>
    /// <example>
    /// <code>var model = new EntityModel("Trip", "Demo.Domain", properties, filters, true, null, null, false, true);</code>
    /// </example>
    public EntityModel(
        string name,
        string @namespace,
        EquatableArray<PropertyModel> properties,
        EquatableArray<PropertyModel> filterProperties,
        bool isAggregateRoot,
        string? mapToTypeName,
        string? mapToTypeNamespace,
        bool isSoftDelete,
        bool isAuditable)
    {
        Name = name;
        Namespace = @namespace;
        Properties = properties;
        FilterProperties = filterProperties;
        IsAggregateRoot = isAggregateRoot;
        MapToTypeName = mapToTypeName;
        MapToTypeNamespace = mapToTypeNamespace;
        IsSoftDelete = isSoftDelete;
        IsAuditable = isAuditable;
    }

    /// <summary>
    /// Determines whether this instance is equal to another entity model.
    /// </summary>
    /// <param name="other">The entity model to compare.</param>
    /// <returns><see langword="true"/> when all value-equal fields match; otherwise <see langword="false"/>.</returns>
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
            && string.Equals(MapToTypeNamespace, other.MapToTypeNamespace, StringComparison.Ordinal)
            && IsSoftDelete == other.IsSoftDelete
            && IsAuditable == other.IsAuditable;
    }

    /// <summary>
    /// Determines whether this instance is equal to another object.
    /// </summary>
    /// <param name="obj">The object to compare.</param>
    /// <returns><see langword="true"/> when the object is a matching entity model; otherwise <see langword="false"/>.</returns>
    public override bool Equals(object? obj) => Equals(obj as EntityModel);

    /// <summary>
    /// Computes a stable hash code for incremental cache invalidation.
    /// </summary>
    /// <returns>The hash code for this entity model.</returns>
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
            hash = (hash ^ IsSoftDelete.GetHashCode()) * 16777619;
            hash = (hash ^ IsAuditable.GetHashCode()) * 16777619;
            return hash;
        }
    }
}
