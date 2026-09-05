using DialogueSystem.Util;
using UnityEngine;

namespace DialogueSystem.Scriptable.Methods
{
    [CreateAssetMenu(menuName = "IIM/Dialogue/Text/Method/Multiplier")]
    public class MultiplierMethodScriptable : DialogueMethodScriptable
    {
        public override string Identifier => nameof(Multiply);
        public override string Separator => "*";
    }
}