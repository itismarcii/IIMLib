using System;
using System.Linq;
using IIMLib.Core.TypeHierarchy;

namespace IIMLib.Core.Module
{
    public static class ModuleExtension
    {
        public static T AddModule<T>(this IModuleHolder holder, T module) where T : class, IModule
        {
            if (holder == null)
                throw new ArgumentNullException(nameof(holder));

            holder.AddModule(module);
            return module;
        }

        public static bool RemoveModule<T>(this IModuleHolder holder) where T : class, IModule
        {
            if (holder == null)
                throw new ArgumentNullException(nameof(holder));

            if (!holder.TryGetModule<T>(out var module))
                return false;

            holder.RemoveModule(module);
            return true;
        }

        public static bool TryGetModule<T>(this IModuleHolder holder, out T module) where T : class, IModule
        {
            if (holder == null)
                throw new ArgumentNullException(nameof(holder));

            var requestedType = typeof(T);
            var modules = holder.Modules;

            for (var i = 0; i < modules.Count; i++)
            {
                var candidate = modules[i];
                if (candidate is T typed)
                {
                    module = typed;
                    return true;
                }

                if (IsAssignableTo(candidate.GetType(), requestedType))
                {
                    module = candidate as T;
                    if (module != null)
                        return true;
                }
            }

            module = null;
            return false;
        }

        public static T GetModule<T>(this IModuleHolder holder) where T : class, IModule
        {
            if (holder.TryGetModule<T>(out var module))
                return module;

            throw new InvalidOperationException($"Module '{typeof(T).FullName}' is not attached.");
        }

        private static bool IsAssignableTo(Type candidateType, Type requestedType)
        {
            if (requestedType.IsAssignableFrom(candidateType))
                return true;

            return TypeHierarchyCache.GetBaseTypes(candidateType).Contains(requestedType) ||
                   TypeHierarchyCache.GetDerivedTypes(candidateType).Contains(requestedType);
        }
    }
}
