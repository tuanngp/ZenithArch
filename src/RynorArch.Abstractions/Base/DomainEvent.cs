using RynorArch.Abstractions.Interfaces;

namespace RynorArch.Abstractions.Base;

/// <summary>
/// Base record for domain events. Immutable by design.
/// </summary>
public abstract record DomainEvent : IDomainEvent
{
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}
