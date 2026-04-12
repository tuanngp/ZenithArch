using RynorArch.Abstractions.Interfaces;

namespace RynorArch.Abstractions.Base;

/// <summary>
/// Base class for all domain entities. Provides identity and domain event support.
/// </summary>
public abstract class EntityBase : IAggregateRoot
{
    /// <summary>
    /// Gets the unique identifier of the entity.
    /// </summary>
    public Guid Id { get; init; }

    private readonly List<IDomainEvent> _domainEvents = [];

    /// <summary>
    /// Gets the domain events raised by the entity since the last clear operation.
    /// </summary>
    public IReadOnlyCollection<IDomainEvent> DomainEvents => _domainEvents.AsReadOnly();

    /// <summary>
    /// Adds a domain event to the in-memory event buffer.
    /// </summary>
    /// <param name="domainEvent">The domain event instance to buffer.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="domainEvent"/> is <see langword="null"/>.</exception>
    /// <example>
    /// <code>entity.RaiseDomainEvent(new TripCreatedEvent(entity.Id));</code>
    /// </example>
    public void RaiseDomainEvent(IDomainEvent domainEvent)
    {
        if (domainEvent is null)
        {
            throw new ArgumentNullException(nameof(domainEvent));
        }

        _domainEvents.Add(domainEvent);
    }

    /// <summary>
    /// Removes all buffered domain events from the entity.
    /// </summary>
    /// <example>
    /// <code>entity.ClearDomainEvents();</code>
    /// </example>
    public void ClearDomainEvents()
    {
        _domainEvents.Clear();
    }
}
