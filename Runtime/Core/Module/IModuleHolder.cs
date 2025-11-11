using System.Collections.Generic;
using System.Linq;
using IIMLib.Extension.TypeHierarchy;

namespace IIMLib.Core.Module
{
    public interface IModuleHolder
    {
        protected List<IModule> ModulesList { get; }
        IReadOnlyList<IModule> Modules => ModulesList;

        public void AddModule<T>(T module, bool checkForDuplication = true) where T : class, IModule
        {
            if(module == null) return;
            
            if(checkForDuplication && IsModuleDuplicationInvalid(module)) return;

            module.Owner = this;
            ModulesList.Add(module);
        }

        public void RemoveModule<T>(bool removeAbsolute = true) where T : class, IModule
        {
            var targetType = typeof(T);

            if (removeAbsolute)
            {
                for (var i = ModulesList.Count - 1; i >= 0; i--)
                {
                    var mod = ModulesList[i];
                    
                    if (!ModuleExtension.IsAssignableTo(mod.GetType(), targetType)) continue;
                    
                    mod.OnRemove();
                    ModulesList.RemoveAt(i);
                }
            }
            else
            {
                for (var i = 0; i < ModulesList.Count; i++)
                {
                    var mod = ModulesList[i];

                    if (!ModuleExtension.IsAssignableTo(mod.GetType(), targetType)) continue;
                    
                    mod.OnRemove();
                    ModulesList.RemoveAt(i);
                    return;
                }
            }
        }

        bool IsModuleDuplicationInvalid<T>(T module) where T : class, IModule
        {
            var type = typeof(T);
            var baseTypes = TypeHierarchyCache.GetBaseTypes(type);
            var derivedTypes = TypeHierarchyCache.GetDerivedTypes(type);
            
            foreach (var mod in Modules)
            {
                var modType = mod.GetType();
                
                if (modType == module.GetType()) return true;
                if (baseTypes.Any(t => t == modType)) return true;
                if (derivedTypes.Any(t => t == modType)) return true;
            }

            return false;
        }
    }
}