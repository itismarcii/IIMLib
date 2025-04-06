using System;
using System.Collections.Generic;
using UnityEngine;

namespace IIMLib.Core.Module
{
    public class ModuleHandler
    {
        private static readonly Dictionary<GameObject, Dictionary<Type, List<IModule>>> Modules = new();
        private static readonly Dictionary<Type, Type[]> TypeInheritanceCache = new();
        
        public static void RemoveModule<T>(in GameObject owner) where T : IModule
        {
            if (!Modules.TryGetValue(owner, out var modules) || !modules.TryGetValue(typeof(T), out var moduleList)) return;

            foreach (var module in moduleList)
            {
                module.OnRemove();
            }

            foreach (var type in GetInheritedTypes(typeof(T)))
            {
                modules.Remove(type);
            }
        }
        
        public static T GetModuleUnsafe<T>(in GameObject owner) where T : class, IModule =>
            Modules.TryGetValue(owner, out var modules) &&
            modules.TryGetValue(typeof(T), out var moduleList) && moduleList.Count > 0
                ? moduleList[0] as T
                : null;
        
        public static T GetModule<T>(in GameObject owner) where T : class, IModule => GetModule<T>(owner, out var output) ? output : null;

        public static bool GetModule<T>(in GameObject owner, out T module) where T : class, IModule
        {
            module = null;

            if (!Modules.TryGetValue(owner, out var modules) || !modules.TryGetValue(typeof(T), out var moduleList) ||
                moduleList.Count <= 0) return false;
        
            module = moduleList[0] as T;
            return true;
        }
        
        public bool HasModule<T>(in GameObject owner) where T : IModule =>
            Modules.TryGetValue(owner, out var modules) && modules.ContainsKey(typeof(T));

        public bool AddModule<T>(in GameObject owner, in T module) where T : class, IModule
        {
            module.Owner = owner;

            if (!Modules.TryGetValue(owner, out var modules))
            {
                modules = new Dictionary<Type, List<IModule>>();
                Modules[owner] = modules;
            }

            var inheritedTypes = GetInheritedTypes(typeof(T));
            foreach (var type in inheritedTypes)
            {
                if (!modules.TryGetValue(type, out var moduleList))
                {
                    moduleList = new List<IModule>();
                    modules[type] = moduleList;
                }
                moduleList.Add(module);
            }

            return true;
        }
    
        private static Type[] GetInheritedTypes(Type type)
        {
            if (TypeInheritanceCache.TryGetValue(type, out var cachedTypes)) return cachedTypes;

            var types = new List<Type> { type };
            types.AddRange(type.GetInterfaces());

            var baseType = type.BaseType;
            while (baseType != null && typeof(IModule).IsAssignableFrom(baseType))
            {
                types.Add(baseType);
                baseType = baseType.BaseType;
            }

            TypeInheritanceCache[type] = types.ToArray();
            return TypeInheritanceCache[type];
        }
    }
}
