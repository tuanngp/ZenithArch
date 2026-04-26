namespace ZenithArch.Abstractions.Interfaces;

/// <summary>
/// Marker interface for domain events.
/// </summary>
public interface IDomainEvent
{
    /// <summary>
    /// Gets the UTC timestamp when the event was raised.
    /// </summary>
    DateTime OccurredOn { get; }
}
