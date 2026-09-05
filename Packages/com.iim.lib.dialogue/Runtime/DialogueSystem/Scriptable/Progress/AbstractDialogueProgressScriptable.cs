using DialogueSystem.Progress;
using UnityEngine;

namespace DialogueSystem.Scriptable.Progress
{
    public abstract class AbstractDialogueProgressScriptable : ScriptableObject
    {
        public abstract IDialogueProgress Progress { get; }
    }
}