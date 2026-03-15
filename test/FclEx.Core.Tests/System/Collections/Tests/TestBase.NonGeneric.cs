// ReSharper disable all
// ReSharper disable InvalidXmlDocComment
#nullable disable
#pragma warning disable CA1068 // CancellationToken parameters must come last
#pragma warning disable CA2263 // Prefer generic overload when type is known
#pragma warning disable CS8632 // The annotation for nullable reference types should only be used in code within a '#nullable' annotations context.
#pragma warning disable IDE0002 // Simplify member access.
#pragma warning disable IDE0005 // Using directive is unnecessary.
#pragma warning disable IDE0019 // Use pattern matching
#pragma warning disable IDE0034 // Simplify 'default' expression
#pragma warning disable IDE0044 // Add readonly modifier
#pragma warning disable IDE0057 // Use range operator
#pragma warning disable IDE0066 // Convert switch statement to expression
#pragma warning disable IDE0251 // Make member 'readonly'
#pragma warning disable IDE0301 // Use collection expression for empty
#pragma warning disable IDE0350 // Use implicitly typed lambda
#pragma warning disable xUnit2015 // Do not use typeof expression to check the exception type.

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
