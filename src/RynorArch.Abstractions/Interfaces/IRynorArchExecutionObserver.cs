namespace RynorArch.Abstractions.Interfaces;

/// <summary>
/// Optional observer hook for generated runtime execution flow.
/// Register one or more implementations to collect logs/telemetry.
/// </summary>
public interface IRynorArchExecutionObserver
{
    /// <summary>
    /// Called immediately before a generated handler executes.
    /// </summary>
    /// <param name="operation">The operation name, such as Create, Update, or Query.</param>
    /// <param name="entityName">The target entity name.</param>
    /// <param name="entityId">The target entity identifier when known.</param>
    /// <param name="userId">The current user identifier when available.</param>
    /// <param name="tenantId">The current tenant identifier when available.</param>
    void OnHandlerExecuting(string operation, string entityName, Guid? entityId, string? userId, string? tenantId);

    /// <summary>
    /// Called after a generated handler completes execution.
    /// </summary>
    /// <param name="operation">The operation name, such as Create, Update, or Query.</param>
    /// <param name="entityName">The target entity name.</param>
    /// <param name="entityId">The target entity identifier when known.</param>
    /// <param name="success"><see langword="true"/> when handler execution completed successfully; otherwise <see langword="false"/>.</param>
    void OnHandlerCompleted(string operation, string entityName, Guid? entityId, bool success);

    /// <summary>
    /// Called when generated request validation fails.
    /// </summary>
    /// <param name="requestName">The validated request type name.</param>
    /// <param name="failureCount">The number of validation failures.</param>
    void OnValidationFailed(string requestName, int failureCount);
}
