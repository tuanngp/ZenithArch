using System;
using System.Collections;
using System.Collections.Generic;

namespace RynorArch.Generator.Models;

/// <summary>
/// An immutable array wrapper that implements value equality via SequenceEqual.
/// Critical for incremental generator correctness — without this, the pipeline
/// would re-execute on every keystroke because arrays use reference equality.
/// </summary>
public readonly struct EquatableArray<T> : IEquatable<EquatableArray<T>>, IEnumerable<T>
    where T : IEquatable<T>
{
    private readonly T[]? _array;

    /// <summary>
    /// Initializes a new instance of the <see cref="EquatableArray{T}"/> struct from an array.
    /// </summary>
    /// <param name="array">The array to wrap.</param>
    /// <example>
    /// <code>var wrapped = new EquatableArray&lt;PropertyModel&gt;(array);</code>
    /// </example>
    public EquatableArray(T[] array)
    {
        _array = array;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="EquatableArray{T}"/> struct from a read-only list.
    /// </summary>
    /// <param name="list">The source list to copy.</param>
    public EquatableArray(IReadOnlyList<T> list)
    {
        var arr = new T[list.Count];
        for (int i = 0; i < list.Count; i++)
        {
            arr[i] = list[i];
        }
        _array = arr;
    }

    /// <summary>
    /// Gets an empty equatable array instance.
    /// </summary>
    public static EquatableArray<T> Empty { get; } = new(Array.Empty<T>());

    /// <summary>
    /// Returns the wrapped array, or an empty array when uninitialized.
    /// </summary>
    /// <returns>The underlying array.</returns>
    public T[] AsArray() => _array ?? Array.Empty<T>();

    /// <summary>
    /// Gets the number of items in the wrapped array.
    /// </summary>
    public int Length => _array?.Length ?? 0;

    /// <summary>
    /// Gets the item at the provided index.
    /// </summary>
    /// <param name="index">Zero-based item index.</param>
    /// <returns>The item at the requested index.</returns>
    public T this[int index] => AsArray()[index];

    /// <summary>
    /// Determines whether this instance equals another equatable array instance.
    /// </summary>
    /// <param name="other">The other instance to compare.</param>
    /// <returns><see langword="true"/> when both arrays have the same sequence of values.</returns>
    public bool Equals(EquatableArray<T> other)
    {
        return AsSpan().SequenceEqual(other.AsSpan());
    }

    /// <summary>
    /// Determines whether this instance equals another object.
    /// </summary>
    /// <param name="obj">The object to compare.</param>
    /// <returns><see langword="true"/> when the object is an equal equatable array.</returns>
    public override bool Equals(object? obj)
    {
        return obj is EquatableArray<T> other && Equals(other);
    }

    /// <summary>
    /// Computes a stable hash code based on contained item values.
    /// </summary>
    /// <returns>The hash code for the wrapped value sequence.</returns>
    public override int GetHashCode()
    {
        var array = AsArray();
        if (array.Length == 0) return 0;

        // Use FNV-1a for fast, low-collision hashing
        unchecked
        {
            int hash = (int)2166136261;
            for (int i = 0; i < array.Length; i++)
            {
                hash = (hash ^ array[i].GetHashCode()) * 16777619;
            }
            return hash;
        }
    }

    /// <summary>
    /// Returns a read-only span over the wrapped array.
    /// </summary>
    /// <returns>A read-only span over contained items.</returns>
    public ReadOnlySpan<T> AsSpan() => AsArray().AsSpan();

    /// <summary>
    /// Compares two arrays for value equality.
    /// </summary>
    /// <param name="left">The left operand.</param>
    /// <param name="right">The right operand.</param>
    /// <returns><see langword="true"/> when both operands are value-equal.</returns>
    public static bool operator ==(EquatableArray<T> left, EquatableArray<T> right)
        => left.Equals(right);

    /// <summary>
    /// Compares two arrays for value inequality.
    /// </summary>
    /// <param name="left">The left operand.</param>
    /// <param name="right">The right operand.</param>
    /// <returns><see langword="true"/> when operands are not value-equal.</returns>
    public static bool operator !=(EquatableArray<T> left, EquatableArray<T> right)
        => !left.Equals(right);

    /// <summary>
    /// Returns a strongly typed enumerator for the wrapped array.
    /// </summary>
    /// <returns>An enumerator over contained items.</returns>
    public IEnumerator<T> GetEnumerator()
    {
        var array = AsArray();
        for (int i = 0; i < array.Length; i++)
        {
            yield return array[i];
        }
    }

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}
