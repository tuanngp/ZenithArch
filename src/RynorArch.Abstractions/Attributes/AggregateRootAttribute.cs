namespace RynorArch.Abstractions.Attributes;

/// <summary>
/// Marks an entity as a DDD Aggregate Root.
/// Enables domain event infrastructure generation.
/// Must be used in conjunction with <see cref="EntityAttribute"/>.
/// </summary>
[AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
public sealed class AggregateRootAttribute : Attribute;
