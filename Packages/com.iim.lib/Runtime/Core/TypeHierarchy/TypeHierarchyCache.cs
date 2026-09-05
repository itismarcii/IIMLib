using System;
using System.Collections.Generic;
using System.Linq;

namespace IIMLib.Core.TypeHierarchy
{
    public static class TypeHierarchyCache
    {
        private static readonly Dictionary<Type, HashSet<Type>> BaseTypes = new();
        private static readonly Dictionary<Type, HashSet<Type>> DerivedTypes = new();
        private static readonly object Sync = new();

        private static bool _initialized;

        public static IReadOnlyCollection<Type> GetBaseTypes(Type type)
        {
            if (type == null)
                throw new ArgumentNullException(nameof(type));

            EnsureInitialized();

            return BaseTypes.TryGetValue(type, out var types)
                ? types
                : Array.Empty<Type>();
        }

        public static IReadOnlyCollection<Type> GetDerivedTypes(Type type)
        {
            if (type == null)
                throw new ArgumentNullException(nameof(type));

            EnsureInitialized();

            return DerivedTypes.TryGetValue(type, out var types)
                ? types
                : Array.Empty<Type>();
        }

        private static void EnsureInitialized()
        {
            if (_initialized)
                return;

            lock (Sync)
            {
                if (_initialized)
                    return;

                Initialize();
                _initialized = true;
            }
        }

        private static void Initialize()
        {
            var allTypes = AppDomain.CurrentDomain
                .GetAssemblies()
                .SelectMany(HelperCollection.Reflection.SafeGetTypes)
                .Where(static type => type != null)
                .ToArray();

            foreach (var type in allTypes)
            {
                var baseChain = BuildBaseChainWithReuse(type);
                BaseTypes[type] = baseChain;

                foreach (var baseType in baseChain)
                {
                    if (!DerivedTypes.TryGetValue(baseType, out var derived))
                    {
                        derived = new HashSet<Type>();
                        DerivedTypes.Add(baseType, derived);
                    }

                    derived.Add(type);
                }

                foreach (var implementedInterface in type.GetInterfaces())
                {
                    if (!BaseTypes[type].Contains(implementedInterface))
                        BaseTypes[type].Add(implementedInterface);

                    if (!DerivedTypes.TryGetValue(implementedInterface, out var implementations))
                    {
                        implementations = new HashSet<Type>();
                        DerivedTypes.Add(implementedInterface, implementations);
                    }

                    implementations.Add(type);
                }
            }
        }

        private static HashSet<Type> BuildBaseChainWithReuse(Type type)
        {
            var result = new HashSet<Type>();
            var current = type.BaseType;

            while (current != null)
            {
                result.Add(current);

                if (BaseTypes.TryGetValue(current, out var cached))
                {
                    result.UnionWith(cached);
                    break;
                }

                current = current.BaseType;
            }

            return result;
        }
    }
}
