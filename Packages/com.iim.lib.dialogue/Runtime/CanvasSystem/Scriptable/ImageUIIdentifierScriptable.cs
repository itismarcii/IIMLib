using System;
using UnityEngine;
using UnityEngine.UI;

namespace CanvasSystem.Scriptable
{
    [CreateAssetMenu(menuName = "IIM/Identifier/Canvas/Image")]
    public class ImageUIIdentifierScriptable : UIIdentifierScriptable
    {
        public override GameObject UIGameObject => UI?.gameObject;
        public override Type Type => typeof(Image);

        [field: SerializeField] public Image UI { get; private set; }
    }
}