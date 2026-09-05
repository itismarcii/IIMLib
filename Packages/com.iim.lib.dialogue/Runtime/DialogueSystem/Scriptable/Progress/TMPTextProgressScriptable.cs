using CanvasSystem.Scriptable;
using DialogueSystem.Progress;
using DialogueSystem.Progress.Util;
using UnityEngine;

namespace DialogueSystem.Scriptable.Progress
{
    [CreateAssetMenu(menuName = "IIM/Dialogue/Progress/TMP_Text")]
    public class TMPTextProgressScriptable : AbstractDialogueProgressScriptable
    {
        private TMPTextProgress _Progress;
        public override IDialogueProgress Progress => _Progress ??= new TMPTextProgress(Text, DisplayText);

        [field: SerializeField] public AbstractDialogueTextScriptable Text { get; private set; }
        [field: SerializeField] public TMPTextIdentifierScriptable DisplayText { get; private set; }
    }
}
