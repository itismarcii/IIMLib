using UnityEngine;

namespace IIMLib.Core.Module
{
    public interface IModule
    {
        public GameObject Owner { get; set; }
        public void OnRemove();
    }
}