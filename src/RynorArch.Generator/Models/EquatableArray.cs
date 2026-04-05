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

    public EquatableArray(T[] array)
    {
        _array = array;
    }

    public EquatableArray(IReadOnlyList<T> list)
    {
        var arr = new T[list.Count];
        for (int i = 0; i < list.Count; i++)
        {
            arr[i] = list[i];
        }
        _array = arr;
    }

    public static EquatableArray<T> Empty { get; } = new(Array.Empty<T>());

    public T[] AsArray() => _array ?? Array.Empty<T>();

    public int Length => _array?.Length ?? 0;

    public T this[int index] => AsArray()[index];

    public bool Equals(EquatableArray<T> other)
    {
        return AsSpan().SequenceEqual(other.AsSpan());
    }

    public override bool Equals(object? obj)
    {
        return obj is EquatableArray<T> other && Equals(other);
    }

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

    public ReadOnlySpan<T> AsSpan() => AsArray().AsSpan();

    public static bool operator ==(EquatableArray<T> left, EquatableArray<T> right)
        => left.Equals(right);

    public static bool operator !=(EquatableArray<T> left, EquatableArray<T> right)
        => !left.Equals(right);

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
