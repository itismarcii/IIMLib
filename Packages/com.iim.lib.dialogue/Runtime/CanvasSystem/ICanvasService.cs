using IIMLib.Core;
using UnityEngine;

namespace CanvasSystem
{
    public interface ICanvasService : IService
    {
        public bool TryGetUI<T>(IUIIdentifier identifier, out T element);
        public GameObject GetUI(IUIIdentifier identifier);
        public Canvas GetCanvas(ICanvasIdentifier identifier);
    }
}