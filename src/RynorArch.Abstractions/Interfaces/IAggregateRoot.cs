namespace RynorArch.Abstractions.Interfaces;

/// <summary>
/// Marker interface for DDD Aggregate Roots.
/// Provides domain event collection management.
/// </summary>
public interface IAggregateRoot
{
    /// <summary>
    /// Gets the buffered domain events for this aggregate root.
    /// </summary>
    IReadOnlyCollection<IDomainEvent> DomainEvents { get; }

    /// <summary>
    /// Clears all buffered domain events from this aggregate root.
    /// </summary>
    void ClearDomainEvents();
}
