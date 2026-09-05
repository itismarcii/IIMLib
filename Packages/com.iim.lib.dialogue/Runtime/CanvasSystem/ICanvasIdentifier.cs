using UnityEngine;

namespace CanvasSystem
{
    public interface ICanvasIdentifier : IIdentifier
    {
        public GameObject GameObject => CanvasObject.gameObject;
        public Canvas CanvasObject { get; }
        public bool Create { get; }
        public bool IsDefaultActive { get; }
    }
}