using System;

namespace RynorArch.Abstractions.Interfaces;

/// <summary>
/// Marks an entity for audit tracking of creation and modification.
/// </summary>
public interface IAuditable
{
    DateTime CreatedAt { get; set; }
    string? CreatedBy { get; set; }
    DateTime? LastModifiedAt { get; set; }
    string? LastModifiedBy { get; set; }
}
