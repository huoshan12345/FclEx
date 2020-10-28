using System;
using System.Collections.Generic;
using System.Text;
using Combinatorics.Collections;

namespace FclEx.Extensions.Combinatorics.Collections
{
    public static class EnumerableExtensions
    {
        public static Permutations<T> ToPermutations<T>(this IEnumerable<T> enumerable, GenerateOption type = GenerateOption.WithRepetition)
        {
            return new Permutations<T>(enumerable, type);
        }

        public static Combinations<T> ToCombinations<T>(this IEnumerable<T> enumerable, int lowerIndex, GenerateOption type = GenerateOption.WithRepetition)
        {
            return new Combinations<T>(enumerable, lowerIndex, type);
        }

        public static Variations<T> ToVariations<T>(this IEnumerable<T> enumerable, int lowerIndex, GenerateOption type = GenerateOption.WithRepetition)
        {
            return new Variations<T>(enumerable, lowerIndex, type);
        }
    }
}
