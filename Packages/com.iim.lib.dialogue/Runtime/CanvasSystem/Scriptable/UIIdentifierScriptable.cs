using System;
using System.Collections.Generic;
using UnityEngine;

namespace CanvasSystem.Scriptable
{
    public abstract class UIIdentifierScriptable : ScriptableObject, IUIIdentifier
    {
        public static readonly HashSet<IUIIdentifier> Identifiers = new();
        public abstract GameObject UIGameObject { get; }

        public abstract Type Type { get; }
        
        private void OnEnable()
        {
            Identifiers.Add(this);
        }
    }
}
