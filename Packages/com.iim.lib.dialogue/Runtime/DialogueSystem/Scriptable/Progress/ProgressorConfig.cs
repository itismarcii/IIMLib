using System.Linq;
using DialogueSystem.Progress;
using UnityEngine;

namespace DialogueSystem.Scriptable.Progress
{
    [CreateAssetMenu(menuName = "IIM/Dialogue/Progress/Config")]
    public class ProgressorConfig : ScriptableObject
    {
        private IDialogueProgress[] _Config;

        public IDialogueProgress[] Config => _Config ??= Progresses.Select(progress => progress.Progress).ToArray();
        
        [field: SerializeField] public AbstractDialogueProgressScriptable[] Progresses { get; private set; }
    }
}