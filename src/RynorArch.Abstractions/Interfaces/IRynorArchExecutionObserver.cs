namespace RynorArch.Abstractions.Interfaces;

/// <summary>
/// Optional observer hook for generated runtime execution flow.
/// Register one or more implementations to collect logs/telemetry.
/// </summary>
public interface IRynorArchExecutionObserver
{
    void OnHandlerExecuting(string operation, string entityName, Guid? entityId, string? userId, string? tenantId);
    void OnHandlerCompleted(string operation, string entityName, Guid? entityId, bool success);
    void OnValidationFailed(string requestName, int failureCount);
}
