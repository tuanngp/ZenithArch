namespace RynorArch.Abstractions.Interfaces;

/// <summary>
/// Optional per-request security context for generated handlers.
/// Register an implementation in DI to propagate user and tenant metadata.
/// </summary>
public interface ISecurityContext
{
    string? UserId { get; }
    string? TenantId { get; }
}
