using System;
using System.Collections.Generic;
using System.Linq;
using IIMLib.Core;
using UnityEngine;
using Object = UnityEngine.Object;

namespace IIMLib.Loop
{
    [CreateAssetMenu(fileName = "ServiceConfigHolder", menuName = "IIM/Config/Service/ServiceConfigHolder")]
    public class ServiceConfigHolder : ServiceConfigIIMAbstract
    {
        [SerializeField, RequireInterface(typeof(IServiceConfigComponent))] private Object[] _Components;

        public IServiceConfigComponent[] Components => HelperCollection.ToInterfaceArray<IServiceConfigComponent>(_Components);

        protected override IEnumerable<(Type, IService)> GetList()
        {
            var hashSet = new HashSet<Type>(Components.Select(c => c.Config.key));
            
            foreach (var (key, service) in base.GetList())
            {
                if(hashSet.Contains(key)) continue;
                yield return (key, service);
            }

            foreach (var component in Components)
            {
                yield return component.Config;
            }
        }
    }
}