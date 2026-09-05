using IIMLib.Core;
using UnityEngine;

namespace DialogueSystem.Scriptable
{
    public abstract class AbstractDialogueTextScriptable : ScriptableObject, IDialogueText
    {
        protected static IDialogueService DialogueService => ServiceLocator.Get<IDialogueService>();
        
        public virtual string Text => DialogueService.ResolveDialogueString(GetTextString());
        protected abstract string GetTextString();
    }
}
