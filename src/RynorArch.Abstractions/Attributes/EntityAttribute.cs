namespace RynorArch.Abstractions.Attributes;

/// <summary>
/// Marks a class as a domain entity for source generation.
/// The generator will produce infrastructure code based on the configured architecture pattern.
/// </summary>
[AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
public sealed class EntityAttribute : Attribute;
