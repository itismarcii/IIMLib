using UnityEngine;

namespace IIMLib.Core.Module
{
    public abstract class BaseModule : IModule
    {
        public GameObject Owner { get; set; }
        public virtual void OnRemove() { }
    }
}