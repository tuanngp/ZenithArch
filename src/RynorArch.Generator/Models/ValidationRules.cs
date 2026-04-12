using System;

namespace RynorArch.Generator.Models;

/// <summary>
/// Holds normalized validation settings extracted from property attributes.
/// </summary>
public sealed class ValidationRules : IEquatable<ValidationRules>
{
    /// <summary>
    /// Gets or sets a value indicating whether the property is required.
    /// </summary>
    public bool IsRequired { get; set; }

    /// <summary>
    /// Gets or sets the configured maximum length, when provided.
    /// </summary>
    public int? MaxLength { get; set; }

    /// <summary>
    /// Gets or sets the configured minimum length, when provided.
    /// </summary>
    public int? MinLength { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether email format validation is requested.
    /// </summary>
    public bool IsEmail { get; set; }

    /// <summary>
    /// Determines whether this instance is equal to another validation rule set.
    /// </summary>
    /// <param name="other">The validation rule set to compare.</param>
    /// <returns><see langword="true"/> when all rule values are equal; otherwise <see langword="false"/>.</returns>
    /// <example>
    /// <code>var same = rules.Equals(otherRules);</code>
    /// </example>
    public bool Equals(ValidationRules? other)
    {
        if (other is null) return false;
        return IsRequired == other.IsRequired
            && MaxLength == other.MaxLength
            && MinLength == other.MinLength
            && IsEmail == other.IsEmail;
    }

    /// <summary>
    /// Determines whether this instance is equal to another object.
    /// </summary>
    /// <param name="obj">The object to compare.</param>
    /// <returns><see langword="true"/> when the object is an equal rule set; otherwise <see langword="false"/>.</returns>
    public override bool Equals(object? obj) => Equals(obj as ValidationRules);
    
    /// <summary>
    /// Computes a stable hash code for this validation rule set.
    /// </summary>
    /// <returns>The hash code for this rule set.</returns>
    public override int GetHashCode()
    {
        unchecked
        {
            int hash = 17;
            hash = hash * 23 + IsRequired.GetHashCode();
            hash = hash * 23 + MaxLength.GetHashCode();
            hash = hash * 23 + MinLength.GetHashCode();
            hash = hash * 23 + IsEmail.GetHashCode();
            return hash;
        }
    }
}
