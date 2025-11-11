using System;
using IIMLib.Extension.TypeHierarchy;

namespace IIMLib.Core.Module
{
    public static class ModuleExtension
    {
        public static void AddModule<T>(this IModuleHolder moduleHolder, T module, bool checkForDuplication = true) where T : class, IModule => moduleHolder.AddModule(module, checkForDuplication);
        public static void RemoveModule<T>(this IModuleHolder moduleHolder, bool removeAbsolute = true) where T : class, IModule => moduleHolder.RemoveModule<T>(removeAbsolute);
        
        public static bool TryGetModule<T>(this IModuleHolder moduleHolder, out T module, bool directSearch = false) where T : class, IModule
        {
            module = moduleHolder.GetModule<T>(directSearch);
            return module != null;
        }
        
        public static T GetModule<T>(this IModuleHolder holder, bool directSearch = false) where T : class, IModule
        {
            var targetType = typeof(T);

            foreach (var module in holder.Modules)
            {
                var moduleType = module.GetType();

                if (directSearch)
                {
                    if(moduleType == targetType) return module as T;
                }
                else
                {
                    if(IsAssignableTo(module.GetType(), targetType)) return module as T;
                }
                
            }

            return null;
        }

        public static bool IsAssignableTo(Type type, Type targetBase)
        {
            if (type == targetBase) return true;

            var baseTypes = TypeHierarchyCache.GetBaseTypes(type);

            foreach (var baseType in baseTypes)
            {
                if (targetBase == baseType) return true;
            }
            
            return targetBase.IsInterface && targetBase.IsAssignableFrom(type);
        }
    }
}