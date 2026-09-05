using UnityEngine;

namespace DialogueSystem.Scriptable
{
    [CreateAssetMenu(menuName = "IIM/Dialogue/Text/Logic")]
    public class DialogueMethodStringScriptable : AbstractDialogueMethodStringScriptable
    {
        [field: SerializeField] public string MethodValue { get; private set; }
        
        public override string GetMethodString() => $"{GetTypePrefix(Type)}{Method.Identifier}({MethodValue}){GetTypeSuffix(Type)}";
    }
}