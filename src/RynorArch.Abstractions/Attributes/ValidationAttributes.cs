using System;

namespace RynorArch.Abstractions.Attributes;

/// <summary>
/// Declares the maximum allowed string length for a property in generated validators.
/// </summary>
[AttributeUsage(AttributeTargets.Property)]
public class MaxLengthAttribute : Attribute
{
    /// <summary>
    /// Gets the maximum allowed length.
    /// </summary>
    public int Length { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="MaxLengthAttribute"/> class.
    /// </summary>
    /// <param name="length">The maximum allowed string length.</param>
    /// <example>
    /// <code>[MaxLength(120)] public string Name { get; set; } = string.Empty;</code>
    /// </example>
    public MaxLengthAttribute(int length) => Length = length;
}

/// <summary>
/// Declares the minimum required string length for a property in generated validators.
/// </summary>
[AttributeUsage(AttributeTargets.Property)]
public class MinLengthAttribute : Attribute
{
    /// <summary>
    /// Gets the minimum required length.
    /// </summary>
    public int Length { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="MinLengthAttribute"/> class.
    /// </summary>
    /// <param name="length">The minimum required string length.</param>
    /// <example>
    /// <code>[MinLength(3)] public string Name { get; set; } = string.Empty;</code>
    /// </example>
    public MinLengthAttribute(int length) => Length = length;
}

/// <summary>
/// Marks a property as requiring email format validation in generated validators.
/// </summary>
[AttributeUsage(AttributeTargets.Property)]
public class EmailAttribute : Attribute { }

/// <summary>
/// Marks a property as required in generated validators.
/// </summary>
[AttributeUsage(AttributeTargets.Property)]
public class RequiredAttribute : Attribute { }
