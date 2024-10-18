using System;
using System.Collections.Generic;
using System.Text;

namespace System;

/// <summary>
/// Provides extension methods for type <see cref="Span{T}"/> and <see cref="ReadOnlySpan{T}"/>.
/// </summary>
public static class Span
{
    /// <summary>
    /// Converts contiguous memory identified by the specified pointer
    /// into <see cref="Span{T}"/>.
    /// </summary>
    /// <param name="value">The managed pointer.</param>
    /// <typeparam name="T">The type of the pointer.</typeparam>
    /// <returns>The span of contiguous memory.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static unsafe Span<byte> AsBytes<T>(ref T value) where T : unmanaged
    {
        return Create(ref Unsafe.As<T, byte>(ref value), sizeof(T));
    }

    /// <summary>Creates a new span over a portion of a regular managed object.</summary>
    /// <param name="reference">A reference to data.</param>
    /// <param name="length">The number of <paramref name="T" /> elements that <paramref name="reference" /> contains.</param>
    /// <typeparam name="T">The type of the data items.</typeparam>
    /// <returns>A span.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static unsafe Span<T> Create<T>(ref T reference, int length)
    {
        return new Span<T>(Unsafe.AsPointer(ref reference), length);
    }
}