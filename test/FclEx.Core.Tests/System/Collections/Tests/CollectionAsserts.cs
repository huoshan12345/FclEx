// ReSharper disable all
// ReSharper disable InvalidXmlDocComment
#nullable disable
#pragma warning disable IDE0002 // Simplify member access.
#pragma warning disable IDE0005 // Using directive is unnecessary.
#pragma warning disable IDE0251 // Make member 'readonly'
#pragma warning disable IDE0057 // Use range operator
#pragma warning disable IDE0019 // Use pattern matching
#pragma warning disable IDE0034 // Simplify 'default' expression
#pragma warning disable IDE0350 // Use implicitly typed lambda
#pragma warning disable IDE0044 // Add readonly modifier
#pragma warning disable xUnit2015 // Do not use typeof expression to check the exception type.
#pragma warning disable CS8632 // The annotation for nullable reference types should only be used in code within a '#nullable' annotations context.
#pragma warning disable CA1068 // CancellationToken parameters must come last
#pragma warning disable CA2263 // Prefer generic overload when type is known
#pragma warning disable IDE0066 // Convert switch statement to expression

// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace System.Collections.Tests
{
    internal static class CollectionAsserts
    {
        public static void Equal(ICollection expected, ICollection actual)
        {
            Assert.Equal(expected == null, actual == null);
            if (expected == null)
            {
                return;
            }
            Assert.Equal(expected.Count, actual.Count);
            IEnumerator e = expected.GetEnumerator();
            IEnumerator a = actual.GetEnumerator();
            while (e.MoveNext())
            {
                Assert.True(a.MoveNext(), "actual has fewer elements");
                if (e.Current == null)
                {
                    Assert.Null(a.Current);
                }
                else
                {
                    Assert.IsType(e.Current.GetType(), a.Current);
                    Assert.Equal(e.Current, a.Current);
                }
            }
            Assert.False(a.MoveNext(), "actual has more elements");
        }

        public static void Equal<T>(ICollection<T> expected, ICollection<T> actual)
        {
            Assert.Equal(expected == null, actual == null);
            if (expected == null)
            {
                return;
            }
            Assert.Equal(expected.Count, actual.Count);
            IEnumerator<T> e = expected.GetEnumerator();
            IEnumerator<T> a = actual.GetEnumerator();
            while (e.MoveNext())
            {
                Assert.True(a.MoveNext(), "actual has fewer elements");
                if (e.Current == null)
                {
                    Assert.Null(a.Current);
                }
                else
                {
                    Assert.IsType(e.Current.GetType(), a.Current);
                    Assert.Equal(e.Current, a.Current);
                }
            }
            Assert.False(a.MoveNext(), "actual has more elements");
        }

        public static void EqualUnordered(ICollection expected, ICollection actual)
        {
            Assert.Equal(expected == null, actual == null);
            if (expected == null)
            {
                return;
            }

            // Lookups are an aggregated collections (enumerable contents), but ordered.
            ILookup<object, object> e = expected.Cast<object>().ToLookup(key => key);
            ILookup<object, object> a = actual.Cast<object>().ToLookup(key => key);

            // Dictionaries can't handle null keys, which is a possibility
            Assert.Equal(e.Where(kv => kv.Key != null).ToDictionary(g => g.Key, g => g.Count()), a.Where(kv => kv.Key != null).ToDictionary(g => g.Key, g => g.Count()));

            // Get count of null keys.  Returns an empty sequence (and thus a 0 count) if no null key
            Assert.Equal(e[null].Count(), a[null].Count());
        }

        public static void EqualUnordered<T>(ICollection<T> expected, ICollection<T> actual)
        {
            Assert.Equal(expected == null, actual == null);
            if (expected == null)
            {
                return;
            }

            // Lookups are an aggregated collections (enumerable contents), but ordered.
            ILookup<object, object> e = expected.Cast<object>().ToLookup(key => key);
            ILookup<object, object> a = actual.Cast<object>().ToLookup(key => key);

            // Dictionaries can't handle null keys, which is a possibility
            Assert.Equal(e.Where(kv => kv.Key != null).ToDictionary(g => g.Key, g => g.Count()), a.Where(kv => kv.Key != null).ToDictionary(g => g.Key, g => g.Count()));

            // Get count of null keys.  Returns an empty sequence (and thus a 0 count) if no null key
            Assert.Equal(e[null].Count(), a[null].Count());
        }
    }
}
