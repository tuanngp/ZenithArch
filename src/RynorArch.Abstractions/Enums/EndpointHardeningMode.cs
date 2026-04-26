namespace RynorArch.Abstractions.Enums;

/// <summary>
/// Defines optional hardening behavior for generated minimal API endpoints.
/// </summary>
public enum EndpointHardeningMode
{
    /// <summary>
    /// Keeps generated endpoints minimal and unchanged from baseline behavior.
    /// </summary>
    None = 0,

    /// <summary>
    /// Applies route-level authorization requirements on generated endpoints.
    /// </summary>
    RequireAuthorization = 1,
}
