using IIMLib.Dialogue.Parsing;
using UnityEngine;

namespace IIMLib.Dialogue.Authoring
{
    [CreateAssetMenu(menuName = "IIM/Dialogue/Text", fileName = "DialogueText")]
    public sealed class DialogueTextAsset : AbstractDialogueTextAsset
    {
        [SerializeField, TextArea] private string _text;

        protected override string GetTextString(DialogueSyntax syntax) =>
            _text ?? string.Empty;
    }
}
