using System;
using System.Collections.Generic;
using UnityEngine;

namespace CanvasSystem.Scriptable
{
    [CreateAssetMenu(menuName = "IIM/Identifier/Canvas/Canvas")]
    public class CanvasIdentifierScriptable : IdentifierScriptable, ICanvasIdentifier
    {
        public static readonly HashSet<ICanvasIdentifier> Identifiers = new();
        public override GameObject UIGameObject => CanvasObject.gameObject;
        public override Type Type => typeof(Canvas);

        [field: SerializeField] public bool Create { get; private set; }
        [field: SerializeField] public bool IsDefaultActive { get; private set; }
        [field: SerializeField] public Canvas CanvasObject { get; private set; }

        private void OnEnable()
        {
            Identifiers.Add(this);
        }
    }
}
