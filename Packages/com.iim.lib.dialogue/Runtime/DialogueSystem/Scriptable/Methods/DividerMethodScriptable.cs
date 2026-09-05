using DialogueSystem.Util;
using UnityEngine;

namespace DialogueSystem.Scriptable.Methods
{
    [CreateAssetMenu(menuName = "IIM/Dialogue/Text/Method/Divider")]
    public class DividerMethodScriptable : DialogueMethodScriptable
    {
        public override string Identifier => nameof(Division);
        public override string Separator => "/";
    }
}