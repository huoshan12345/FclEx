using System;
using System.Collections.Generic;
using System.Text;
using Collections;

namespace FclEx
{
    partial class EnumerableExtensions
    {
        public static Permutations<T> ToPermutations<T>(this IEnumerable<T> enumerable, GenerateOption type = GenerateOption.WithRepetition)
        {
            return new(enumerable, type);
        }

        public static Combinations<T> ToCombinations<T>(this IEnumerable<T> enumerable, int lowerIndex, GenerateOption type = GenerateOption.WithRepetition)
        {
            return new(enumerable, lowerIndex, type);
        }

        public static Variations<T> ToVariations<T>(this IEnumerable<T> enumerable, int lowerIndex, GenerateOption type = GenerateOption.WithRepetition)
        {
            return new(enumerable, lowerIndex, type);
        }
    }
}
