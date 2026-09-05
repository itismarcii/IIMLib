using System;
using TMPro;
using UnityEngine;

namespace CanvasSystem.Scriptable
{
    [CreateAssetMenu(menuName = "IIM/Identifier/Canvas/TMP_Text")]
    public class TMPTextIdentifierScriptable : IdentifierScriptable, IUIIdentifier
    {
        public override GameObject UIGameObject => UI?.gameObject;
        public override Type Type => typeof(TMP_Text);

        [field: SerializeField] public TextMeshProUGUI UI { get; private set; }
    }
}
