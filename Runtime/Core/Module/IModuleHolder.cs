using System;
using System.Collections.Generic;
using System.Linq;
using IIMLib.Core.TypeHierarchy;

namespace IIMLib.Core.Module
{
    public interface IModuleHolder
    {
        protected List<IModule> ModulesList { get; }
        public IReadOnlyList<IModule> Modules => ModulesList;

        void AddModule(IModule module)
        {
            if (module == null)
                throw new ArgumentNullException(nameof(module));

            var moduleType = module.GetType();

            for (var i = 0; i < ModulesList.Count; i++)
            {
                var existingType = ModulesList[i].GetType();

                if (existingType == moduleType ||
                    TypeHierarchyCache.GetBaseTypes(existingType).Contains(moduleType) ||
                    TypeHierarchyCache.GetDerivedTypes(existingType).Contains(moduleType))
                {
                    throw new InvalidOperationException(
                        $"A module compatible with '{moduleType.FullName}' is already attached.");
                }
            }

            module.Owner = this;
            ModulesList.Add(module);
        }

        void RemoveModule(IModule module)
        {
            if (module == null || !ModulesList.Remove(module))
                return;

            module.OnRemove();
            module.Owner = null;
        }
    }
}
