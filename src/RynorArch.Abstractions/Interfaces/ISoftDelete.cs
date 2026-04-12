namespace RynorArch.Abstractions.Interfaces;

/// <summary>
/// Marks an entity for soft deletion instead of physical row deletion.
/// </summary>
public interface ISoftDelete
{
    /// <summary>
    /// Gets or sets a value indicating whether the entity is logically deleted.
    /// </summary>
    bool IsDeleted { get; set; }
}
