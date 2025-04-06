using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;

namespace IIMLib.Core
{
    public static class HelperCollection
    {
        private static readonly Random Random = new();

        /// <summary>
        /// Shuffle method that returns enumerable of type T a Fisher-Yates shuffled variant.
        /// </summary>
        /// <returns>Shuffled copy of the input array.</returns>
        /// <exception cref="ArgumentNullException">Input array is null.</exception>
        public static IEnumerable<T> Shuffle<T>(IEnumerable<T>[] array)
        {
            if (array == null) throw new ArgumentNullException(nameof(array));

            var shuffledArray = new T[array.Length];
            Array.Copy(array, shuffledArray, array.Length);

            for (var i = shuffledArray.Length - 1; i > 0; i--)
            {
                var randomIndex = Random.Next(0, i + 1);
                (shuffledArray[i], shuffledArray[randomIndex]) = (shuffledArray[randomIndex], shuffledArray[i]);
            }

            return shuffledArray;
        }

        /// <summary>
        /// Shuffle method that returns a string enumerable of a Fisher-Yates shuffled variant.
        /// </summary>
        /// <returns>Shuffled copy of the input string array.</returns>
        /// <exception cref="ArgumentNullException">Input array is null.</exception>
        public static IEnumerable<string> Shuffle(string[] array)
        {
            if (array == null) throw new ArgumentNullException(nameof(array));

            var shuffledArray = new string[array.Length];
            Array.Copy(array, shuffledArray, array.Length);

            for (var i = shuffledArray.Length - 1; i > 0; i--)
            {
                var randomIndex = Random.Next(0, i + 1);
                (shuffledArray[i], shuffledArray[randomIndex]) = (shuffledArray[randomIndex], shuffledArray[i]);
            }

            return shuffledArray;
        }

        public static Type GetClassByName(string className)
        {
            return AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(assembly => assembly.GetTypes())
                .FirstOrDefault(type =>
                    type.FullName != null &&
                    (type.Name.Equals(className, StringComparison.OrdinalIgnoreCase)));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static byte ParseBool(in bool value) => value ? (byte) 1 : (byte) 0;
        
        /// <summary>
        /// Returns a random bool value with the given probability (between 0 and 1)
        /// </summary>
        public static bool Chance(in float probability) => Random.Next(0,1) < probability;

        /// <summary>
        /// Returns a random element from an array.
        /// </summary>
        public static T RandomElement<T>(T[] array)
        {
            if (array == null || array.Length == 0) throw new ArgumentException("Array is null or empty.");
            return array[Random.Next(array.Length)];
        }

        /// <summary>
        /// Returns a random element from a list.
        /// </summary>
        public static T RandomElement<T>(List<T> list)
        {
            if (list == null || list.Count == 0) throw new ArgumentException("List is null or empty.");
            return list[Random.Next(list.Count)];
        }

        /// <summary>
        /// Swap two elements in an array.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Swap<T>(in T[] array, int indexA, int indexB)
        {
            (array[indexA], array[indexB]) = (array[indexB], array[indexA]);
        }

        /// <summary>
        /// Returns true if the value is between min and max inclusive.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsBetween(int value, int min, int max) => value >= min && value <= max;

        /// <summary>
        /// Returns true if the float value is approximately equal to another.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool Approximately(float a, float b, float tolerance = 0.0001f) => Math.Abs(a - b) < tolerance;

        /// <summary>
        /// Checks whether a type inherits from or implements a base type/interface.
        /// </summary>
        public static bool IsSubclassOfRawGeneric(Type baseType, Type checkType)
        {
            while (checkType != null && checkType != typeof(object))
            {
                var cur = checkType.IsGenericType ? checkType.GetGenericTypeDefinition() : checkType;
                if (baseType == cur) return true;
                checkType = checkType.BaseType;
            }

            return false;
        }

        /// <summary>
        /// Tries to parse an enum by string, case-insensitive.
        /// </summary>
        public static bool TryParseEnum<TEnum>(string value, out TEnum result) where TEnum : struct
        {
            return Enum.TryParse(value, ignoreCase: true, out result);
        }

        /// <summary>
        /// Ensures the array is not null and optionally has a minimum size.
        /// </summary>
        public static bool ValidateArray<T>(T[] array, int minLength = 1)
        {
            return array != null && array.Length >= minLength;
        }

        /// <summary>
        /// Ensures the list is not null and optionally has a minimum size.
        /// </summary>
        public static bool ValidateList<T>(List<T> list, int minCount = 1)
        {
            return list != null && list.Count >= minCount;
        }
    }
}