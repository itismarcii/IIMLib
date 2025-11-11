using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using IIMLib.Core;

namespace IIMLib.Extension.TypeHierarchy
{
    public static class TypeHierarchyCache
    {
        private static readonly Dictionary<Type, Type[]> BaseTypes = new();
        private static readonly Dictionary<Type, HashSet<Type>> DerivedTypes = new();
        
        private static readonly object InitLock = new();
        private static bool _initialized;
        
        public static void Initialize(params Assembly[] assemblies)
        {
            if(_initialized) return;

            lock (InitLock)
            {
                if(_initialized) return;
                
                if(assemblies == null || assemblies.Length == 0) 
                    assemblies = AppDomain.CurrentDomain.GetAssemblies();

                foreach (var type in assemblies.SelectMany(HelperCollection.Reflection.SafeGetTypes))
                {
                    if(!type.IsClass && !type.IsInterface) continue;
                    
                    // Build base type chain using reuse
                    var baseChain = BuildBaseChainWithReuse(type);
                    BaseTypes[type] = baseChain;
                    
                    // Register reverse links (derived types)
                    foreach (var baseType in baseChain)
                    {
                        if (!DerivedTypes.TryGetValue(baseType, out var set))
                        {
                            DerivedTypes[baseType] = set = new HashSet<Type>();
                        }
                        
                        set.Add(type);
                    }
                }
                
                _initialized = true;
            }
        }

        public static Type[] GetBaseTypes(Type type)
        {
            Initialize();
            return BaseTypes.TryGetValue(type, out var array) ?  array : Array.Empty<Type>();
        }

        public static Type[] GetDerivedTypes(Type baseType)
        {
            Initialize();
            return DerivedTypes.TryGetValue(baseType, out var set) ?  set.ToArray() : Array.Empty<Type>();
        }

        private static Type[] BuildBaseChainWithReuse(Type type)
        {
            if(type?.BaseType == null) return Array.Empty<Type>();

            if (BaseTypes.TryGetValue(type.BaseType, out var parentChain))
            {
                var result = new Type[parentChain.Length + 1];
                result[0] = type.BaseType;
                Array.Copy(parentChain,0,result,1,parentChain.Length);
                return result;
            }
            
            // Rare case: walk up manually, cache on way down
            var tempStack = new Stack<Type>();
            var current = type.BaseType;

            while (current != null)
            {
                tempStack.Push(current);

                if (BaseTypes.TryGetValue(current, out var cached))
                {
                    foreach (var cachedBase in cached.Reverse())
                        tempStack.Push(cachedBase);
                    
                    break;
                }
                
                current = current.BaseType;
            }
            
            return tempStack.ToArray();
        }
    }
}