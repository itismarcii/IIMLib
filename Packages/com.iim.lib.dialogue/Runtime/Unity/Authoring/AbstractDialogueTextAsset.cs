using IIMLib.Dialogue.Model;
using IIMLib.Dialogue.Parsing;
using UnityEngine;

namespace IIMLib.Dialogue.Authoring
{
    public abstract class AbstractDialogueTextAsset : ScriptableObject, IDialogueText
    {
        public string Text => GetText(DialogueSyntax.Default);

        public string GetText(DialogueSyntax syntax)
        {
            return GetTextString(syntax ?? DialogueSyntax.Default);
        }

        protected abstract string GetTextString(DialogueSyntax syntax);
    }
}
