using TMPro;
using UnityEngine.UI;

namespace DialogueSystem.Progress.Util
{
    public interface IDialogueTMPTextDisplay
    {
        public IDialogueText Text { get; }
        public TMP_Text DisplayText { get; }
    }
}