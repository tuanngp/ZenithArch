namespace RynorArch.Abstractions.Attributes;

/// <summary>
/// Specifies a mapping target type for the entity.
/// Used to generate mapping logic between domain entity and DTO/ViewModel.
/// </summary>
[AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = true)]
public sealed class MapToAttribute : Attribute
{
    /// <summary>
    /// The target type to map to.
    /// </summary>
    public Type TargetType { get; }

    public MapToAttribute(Type targetType)
    {
        TargetType = targetType;
    }
}
