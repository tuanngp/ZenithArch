using System;

namespace RynorArch.Abstractions.Attributes;

[AttributeUsage(AttributeTargets.Property)]
public class MaxLengthAttribute : Attribute
{
    public int Length { get; }
    public MaxLengthAttribute(int length) => Length = length;
}

[AttributeUsage(AttributeTargets.Property)]
public class MinLengthAttribute : Attribute
{
    public int Length { get; }
    public MinLengthAttribute(int length) => Length = length;
}

[AttributeUsage(AttributeTargets.Property)]
public class EmailAttribute : Attribute { }

[AttributeUsage(AttributeTargets.Property)]
public class RequiredAttribute : Attribute { }
