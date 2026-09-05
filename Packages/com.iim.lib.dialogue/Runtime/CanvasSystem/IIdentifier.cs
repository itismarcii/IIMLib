using System;
using UnityEngine;

namespace CanvasSystem
{
    public interface IIdentifier
    {
        public string Identifier => UIGameObject.gameObject.GetHashCode().ToString();
        public GameObject UIGameObject { get; }
        public Type Type{ get; }
    }
}