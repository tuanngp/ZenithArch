using RynorArch.Abstractions.Interfaces;

namespace RynorArch.Abstractions.Base;

/// <summary>
/// Base class for all domain entities. Provides identity and domain event support.
/// </summary>
public abstract class EntityBase : IAggregateRoot
{
    public Guid Id { get; init; }

    private readonly List<IDomainEvent> _domainEvents = [];

    public IReadOnlyCollection<IDomainEvent> DomainEvents => _domainEvents.AsReadOnly();

    protected void RaiseDomainEvent(IDomainEvent domainEvent)
    {
        _domainEvents.Add(domainEvent);
    }

    public void ClearDomainEvents()
    {
        _domainEvents.Clear();
    }
}
