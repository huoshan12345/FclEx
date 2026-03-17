// ReSharper disable all
#pragma warning disable IDE0002 // Simplify member access.
#pragma warning disable IDE0005 // Using directive is unnecessary.
#pragma warning disable IDE0251 // Make member 'readonly'
#pragma warning disable IDE0057 // Use range operator
#pragma warning disable CS8765 // Nullability of type of parameter doesn't match overridden member (possibly because of nullability attributes).
#pragma warning disable CS8767 // Nullability of reference types in type of parameter doesn't match implicitly implemented member (possibly because of nullability attributes).
#pragma warning disable IDE0019 // Use pattern matching
#pragma warning disable CS8600 // Converting null literal or possible null value to non-nullable type.
#pragma warning disable CS8769 // Nullability of reference types in type of parameter doesn't match implemented member (possibly because of nullability attributes).
#pragma warning disable CA1067 // Override Object.Equals(object) when implementing IEquatable<T>
#pragma warning disable CS8607 // A possible null value may not be used for a type marked with [NotNull] or [DisallowNull]

// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Collections.Generic;

namespace System.Collections.Tests
{
    /// <summary>
    /// Provides a base set of nongeneric operations that are used by all other testing interfaces.
    /// </summary>
    public abstract class TestBase
    {
        #region Helper Methods

        public static IEnumerable<object[]> ValidCollectionSizes()
        {
            yield return new object[] { 0 };
            yield return new object[] { 1 };
            yield return new object[] { 75 };
        }

        public static IEnumerable<object[]> ValidPositiveCollectionSizes()
        {
            yield return new object[] { 1 };
            yield return new object[] { 75 };
        }

        public enum EnumerableType
        {
            HashSet,
            SortedSet,
            List,
            Queue,
            Lazy,
        };

        [Flags]
        public enum ModifyOperation
        {
            None = 0,
            Add = 1,
            Insert = 2,
            Overwrite = 4,
            Remove = 8,
            Clear = 16
        }

        #endregion
    }
}
