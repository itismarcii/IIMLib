using CanvasSystem.Scriptable;
using DialogueSystem.Progress;
using DialogueSystem.Progress.Util;
using UnityEngine;

namespace DialogueSystem.Scriptable.Progress
{
    [CreateAssetMenu(menuName = "IIM/Dialogue/Progress/UI Toggle")]
    public class UIToggleProgressScriptable : AbstractDialogueProgressScriptable
    {
        private IDialogueProgress _Progress;

        public override IDialogueProgress Progress => _Progress ??= new MonoToggleProgress(ToggleObject);
        
        [field: SerializeField] public UIIdentifierScriptable ToggleObject { get; private set; }
    }
}