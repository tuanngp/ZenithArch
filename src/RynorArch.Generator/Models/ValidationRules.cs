using System;

namespace RynorArch.Generator.Models;

public sealed class ValidationRules : IEquatable<ValidationRules>
{
    public bool IsRequired { get; set; }
    public int? MaxLength { get; set; }
    public int? MinLength { get; set; }
    public bool IsEmail { get; set; }

    public bool Equals(ValidationRules? other)
    {
        if (other is null) return false;
        return IsRequired == other.IsRequired
            && MaxLength == other.MaxLength
            && MinLength == other.MinLength
            && IsEmail == other.IsEmail;
    }

    public override bool Equals(object? obj) => Equals(obj as ValidationRules);
    
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
