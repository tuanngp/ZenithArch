namespace RynorArch.Abstractions.Interfaces;

/// <summary>
/// Marks an entity for soft deletion instead of physical row deletion.
/// </summary>
public interface ISoftDelete
{
    bool IsDeleted { get; set; }
}
