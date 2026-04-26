namespace ZenithArch.Abstractions.Attributes;

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

    /// <summary>
    /// Initializes a new instance of the <see cref="MapToAttribute"/> class.
    /// </summary>
    /// <param name="targetType">The destination type used by generated mapping code.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="targetType"/> is <see langword="null"/>.</exception>
    /// <example>
    /// <code>[MapTo(typeof(ProductDto))] public partial class Product : EntityBase { }</code>
    /// </example>
    public MapToAttribute(Type targetType)
    {
        TargetType = targetType ?? throw new ArgumentNullException(nameof(targetType));
    }
}
