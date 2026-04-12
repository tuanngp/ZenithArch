using RynorArch.Abstractions.Interfaces;

namespace RynorArch.Abstractions.Base;

/// <summary>
/// Base record for domain events. Immutable by design.
/// </summary>
public abstract record DomainEvent : IDomainEvent
{
    /// <summary>
    /// Gets the UTC timestamp when the event instance was created.
    /// </summary>
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}
