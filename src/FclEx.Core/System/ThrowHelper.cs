// ReSharper disable All
#pragma warning disable IDE0005

// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

namespace System;

internal static class ThrowHelper
{
    [DoesNotReturn]
    internal static void ThrowInvalidOperationException_EnumCurrent(int index)
    {
        throw GetInvalidOperationException_EnumCurrent(index);
    }

    private static InvalidOperationException GetInvalidOperationException_EnumCurrent(int index)
    {
        return new InvalidOperationException(
            index < 0 ?
            SR.InvalidOperation_EnumNotStarted :
            SR.InvalidOperation_EnumEnded);
    }

    [DoesNotReturn]
    internal static void ThrowKeyNotFound<TKey>(TKey key) =>
        throw new KeyNotFoundException(SR.Format(SR.Arg_KeyNotFoundWithKey, key));

    [DoesNotReturn]
    internal static void ThrowDuplicateKey<TKey>(TKey key) =>
        throw new ArgumentException(SR.Format(SR.Argument_AddingDuplicate, key), nameof(key));

    [DoesNotReturn]
    internal static void ThrowConcurrentOperation() =>
        throw new InvalidOperationException(SR.InvalidOperation_ConcurrentOperationsNotSupported);

    [DoesNotReturn]
    internal static void ThrowIndexArgumentOutOfRange() =>
        throw new ArgumentOutOfRangeException("index");

    [DoesNotReturn]
    internal static void ThrowVersionCheckFailed() =>
        throw new InvalidOperationException(SR.InvalidOperation_EnumFailedVersion);

#if !NET8_0_OR_GREATER
    public static void ThrowIfNull([NotNull] object? argument, [CallerArgumentExpression(nameof(argument))] string? paramName = null)
    {
        if (argument is null)
        {
            ThrowNull(paramName);
        }
    }

    public static void ThrowIfNegative(int value, [CallerArgumentExpression(nameof(value))] string? paramName = null)
    {
        if (value < 0)
        {
            ThrowNegative(value, paramName);
        }
    }

    public static void ThrowIfGreaterThan<T>(T value, T other, [CallerArgumentExpression(nameof(value))] string? paramName = null)
        where T : IComparable<T>
    {
        if (value.CompareTo(other) > 0)
        {
            ThrowGreater(value, other, paramName);
        }
    }

    public static void ThrowIfLessThan<T>(T value, T other, [CallerArgumentExpression(nameof(value))] string? paramName = null)
        where T : IComparable<T>
    {
        if (value.CompareTo(other) < 0)
        {
            ThrowLess(value, other, paramName);
        }
    }

    [DoesNotReturn]
    private static void ThrowNull(string? paramName) =>
        throw new ArgumentNullException(paramName);

    [DoesNotReturn]
    private static void ThrowNegative(int value, string? paramName) =>
        throw new ArgumentOutOfRangeException(paramName, value, SR.Format(SR.ArgumentOutOfRange_Generic_MustBeNonNegative, paramName, value));

    [DoesNotReturn]
    private static void ThrowGreater<T>(T value, T other, string? paramName) =>
        throw new ArgumentOutOfRangeException(paramName, value, SR.Format(SR.ArgumentOutOfRange_Generic_MustBeLessOrEqual, paramName, value, other));

    [DoesNotReturn]
    private static void ThrowLess<T>(T value, T other, string? paramName) =>
        throw new ArgumentOutOfRangeException(paramName, value, SR.Format(SR.ArgumentOutOfRange_Generic_MustBeGreaterOrEqual, paramName, value, other));
#endif
}