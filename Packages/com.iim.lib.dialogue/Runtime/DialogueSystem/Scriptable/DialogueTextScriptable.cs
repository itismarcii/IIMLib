using UnityEngine;

namespace DialogueSystem.Scriptable
{
    [CreateAssetMenu(menuName = "IIM/Dialogue/String")]
    public class DialogueTextScriptable : AbstractDialogueTextScriptable
    {
        [field: SerializeField, TextArea] public string String { get; private set; } 
        protected override string GetTextString() => String;
    }
}