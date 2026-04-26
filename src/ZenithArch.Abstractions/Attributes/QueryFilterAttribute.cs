namespace ZenithArch.Abstractions.Attributes;

/// <summary>
/// Marks a property as a filterable field for specification generation.
/// The generator will include this property in the generated specification class.
/// </summary>
[AttributeUsage(AttributeTargets.Property, Inherited = false, AllowMultiple = false)]
public sealed class QueryFilterAttribute : Attribute;
