using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace IIMLib.Core
{
    public static class HelperCollection
    {
        private static readonly Random Random = new();

        public static IEnumerable<T> Shuffle<T>(IEnumerable<T>[] array)
        {
            if (array == null)
                throw new ArgumentNullException(nameof(array));

            return Shuffle(array.SelectMany(static values => values ?? Array.Empty<T>()).ToArray());
        }

        public static IEnumerable<T> Shuffle<T>(T[] array)
        {
            if (array == null)
                throw new ArgumentNullException(nameof(array));

            var result = new T[array.Length];
            Array.Copy(array, result, array.Length);

            for (var i = result.Length - 1; i > 0; i--)
            {
                var j = Random.Next(i + 1);
                (result[i], result[j]) = (result[j], result[i]);
            }

            return result;
        }

        public static IEnumerable<string> Shuffle(string[] array) => Shuffle<string>(array);

        public static Type GetClassByName(string className)
        {
            if (string.IsNullOrWhiteSpace(className))
                return null;

            return AppDomain.CurrentDomain
                .GetAssemblies()
                .SelectMany(Reflection.SafeGetTypes)
                .FirstOrDefault(type => type.Name == className || type.FullName == className);
        }

        public static bool ParseBool(string value)
        {
            if (bool.TryParse(value, out var result))
                return result;

            if (int.TryParse(value, out var numeric))
                return numeric != 0;

            return !string.IsNullOrWhiteSpace(value);
        }

        public static bool Chance(float probability)
        {
            if (probability <= 0f)
                return false;

            if (probability >= 1f)
                return true;

            return Random.NextDouble() < probability;
        }

        public static T RandomElement<T>(T[] array)
        {
            if (array == null)
                throw new ArgumentNullException(nameof(array));
            if (array.Length == 0)
                throw new ArgumentException("Array must contain at least one element.", nameof(array));

            return array[Random.Next(array.Length)];
        }

        public static T RandomElement<T>(IReadOnlyList<T> list)
        {
            if (list == null)
                throw new ArgumentNullException(nameof(list));
            if (list.Count == 0)
                throw new ArgumentException("List must contain at least one element.", nameof(list));

            return list[Random.Next(list.Count)];
        }

        public static void Swap<T>(IList<T> list, int indexA, int indexB)
        {
            if (list == null)
                throw new ArgumentNullException(nameof(list));

            (list[indexA], list[indexB]) = (list[indexB], list[indexA]);
        }

        public static bool IsBetween<T>(this T value, T min, T max) where T : IComparable<T>
            => value.CompareTo(min) >= 0 && value.CompareTo(max) <= 0;

        public static bool Approximately(float a, float b, float tolerance = 0.0001f)
            => Math.Abs(a - b) <= tolerance;

        public static bool IsSubclassOfRawGeneric(Type generic, Type toCheck)
        {
            while (toCheck != null && toCheck != typeof(object))
            {
                var current = toCheck.IsGenericType ? toCheck.GetGenericTypeDefinition() : toCheck;
                if (generic == current)
                    return true;

                toCheck = toCheck.BaseType;
            }

            return false;
        }

        public static bool TryParseEnum<T>(string value, out T result, bool ignoreCase = true)
            where T : struct, Enum
            => Enum.TryParse(value, ignoreCase, out result);

        public static bool ValidateArray<T>(T[] array)
            => array != null && array.Length > 0;

        public static bool ValidateList<T>(ICollection<T> list)
            => list != null && list.Count > 0;

        public static class Reflection
        {
            public static IEnumerable<Type> SafeGetTypes(Assembly assembly)
            {
                try
                {
                    return assembly.GetTypes();
                }
                catch (ReflectionTypeLoadException exception)
                {
                    return exception.Types.Where(static type => type != null);
                }
            }
        }
        
        public static T[] ToInterfaceArray<T>(object[] objects)
            where T : class
        {
            if (objects == null)
                return Array.Empty<T>();

            var result = new T[objects.Length];

            for (var i = 0; i < objects.Length; i++)
            {
                var obj = objects[i];

                if (obj == null)
                {
                    result[i] = null;
                    continue;
                }

                if (obj is not T value)
                {
                    throw new InvalidCastException(
                        $"Object '{obj}' at index {i} does not implement " +
                        $"'{typeof(T).FullName}'.");
                }

                result[i] = value;
            }

            return result;
        }

        public static bool TryToInterfaceArray<T>(
            object[] objects,
            out T[] result)
            where T : class
        {
            if (objects == null)
            {
                result = Array.Empty<T>();
                return true;
            }

            result = new T[objects.Length];

            for (var i = 0; i < objects.Length; i++)
            {
                var obj = objects[i];

                if (obj == null)
                {
                    result[i] = null;
                    continue;
                }

                if (obj is not T value)
                {
                    result = null;
                    return false;
                }

                result[i] = value;
            }

            return true;
        }
    }
}
