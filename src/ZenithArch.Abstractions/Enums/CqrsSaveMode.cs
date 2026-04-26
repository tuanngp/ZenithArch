namespace ZenithArch.Abstractions.Enums;

/// <summary>
/// Defines persistence behavior for generated CQRS write handlers.
/// </summary>
public enum CqrsSaveMode
{
    /// <summary>
    /// Each write handler persists changes immediately.
    /// </summary>
    PerHandler = 0,

    /// <summary>
    /// Write handlers stage changes and a generated MediatR pipeline behavior commits
    /// once per request inside a transaction.
    /// </summary>
    PerRequestTransaction = 1,
}
