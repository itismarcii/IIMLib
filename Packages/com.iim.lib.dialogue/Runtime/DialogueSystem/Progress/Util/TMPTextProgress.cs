using System;
using CanvasSystem;
using CanvasSystem.Scriptable;
using IIMLib.Core;
using TMPro;
using UnityEngine.UI;

namespace DialogueSystem.Progress.Util
{
    public class TMPTextProgress : IDialogueProgress, IDialogueTMPTextDisplay
    {
        public IDialogueText Text { get; private set; }
        
        public TMP_Text DisplayText { get; private set; }
        
        public bool NeedsUpdate => false;

        public Action OnStart { get; set; }
        public Action OnFinish { get; set; }

        public TMPTextProgress(IDialogueText text, TMPTextIdentifierScriptable identifier)
        {
            Text = text;
            
            if(!ServiceLocator.Get<ICanvasService>().TryGetUI(identifier, out TMPTextIdentifierScriptable textUI)) return;
            
            DisplayText = textUI.UI;
        }
        
        public void Start()
        {
            DisplayText.text = Text.Text;
            OnStart?.Invoke();
            Finish();
        }

        public void Finish()
        {
            OnFinish?.Invoke();
        }

        public void Update(float _) { }

        public void Reset()
        {
            DisplayText.text = string.Empty;
        }
    }
}