namespace ZenithArch.Abstractions.Interfaces;

/// <summary>
/// Optional per-request security context for generated handlers.
/// Register an implementation in DI to propagate user and tenant metadata.
/// </summary>
public interface ISecurityContext
{
    /// <summary>
    /// Gets the current user identifier, when available.
    /// </summary>
    string? UserId { get; }

    /// <summary>
    /// Gets the current tenant identifier, when available.
    /// </summary>
    string? TenantId { get; }
}
