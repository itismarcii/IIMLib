using DialogueSystem.Util;
using UnityEngine;

namespace DialogueSystem.Scriptable.Methods
{
    [CreateAssetMenu(menuName = "IIM/Dialogue/Text/Method/Addition")]
    public class AdditionMethodScriptable : DialogueMethodScriptable
    {
        public override string Identifier => nameof(Addition);
        public override string Separator => "+";
    }
}