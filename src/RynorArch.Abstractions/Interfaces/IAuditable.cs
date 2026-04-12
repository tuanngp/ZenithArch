using System;

namespace RynorArch.Abstractions.Interfaces;

/// <summary>
/// Marks an entity for audit tracking of creation and modification.
/// </summary>
public interface IAuditable
{
    /// <summary>
    /// Gets or sets the UTC timestamp when the entity was created.
    /// </summary>
    DateTime CreatedAt { get; set; }

    /// <summary>
    /// Gets or sets the user identifier that created the entity.
    /// </summary>
    string? CreatedBy { get; set; }

    /// <summary>
    /// Gets or sets the UTC timestamp of the last update.
    /// </summary>
    DateTime? LastModifiedAt { get; set; }

    /// <summary>
    /// Gets or sets the user identifier for the last update.
    /// </summary>
    string? LastModifiedBy { get; set; }
}
