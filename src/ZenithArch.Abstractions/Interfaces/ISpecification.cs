using System.Linq.Expressions;

namespace ZenithArch.Abstractions.Interfaces;

/// <summary>
/// Specification pattern interface for building type-safe query criteria.
/// </summary>
/// <typeparam name="T">The entity type this specification applies to.</typeparam>
public interface ISpecification<T> where T : class
{
    /// <summary>
    /// The filter criteria expression.
    /// </summary>
    Expression<Func<T, bool>>? Criteria { get; }

    /// <summary>
    /// Navigation properties to eagerly load.
    /// </summary>
    IReadOnlyList<Expression<Func<T, object>>> Includes { get; }

    /// <summary>
    /// Number of records to skip for pagination.
    /// </summary>
    int? Skip { get; }

    /// <summary>
    /// Number of records to take for pagination.
    /// </summary>
    int? Take { get; }
}
