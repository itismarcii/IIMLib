using System;
using UnityEngine;

namespace CanvasSystem.Scriptable
{
    public abstract class IdentifierScriptable : ScriptableObject, IIdentifier
    {
        [field: SerializeField] public string IdentifierString { get; private set; }
        public abstract GameObject UIGameObject { get; }
        public abstract Type Type { get; }
        
        private void OnValidate()
        {
            IdentifierString = !UIGameObject ?  string.Empty : ((IIdentifier)this).Identifier;
        }
    }
}