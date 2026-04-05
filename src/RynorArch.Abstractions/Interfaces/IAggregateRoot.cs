namespace RynorArch.Abstractions.Interfaces;

/// <summary>
/// Marker interface for DDD Aggregate Roots.
/// Provides domain event collection management.
/// </summary>
public interface IAggregateRoot
{
    IReadOnlyCollection<IDomainEvent> DomainEvents { get; }

    void ClearDomainEvents();
}
