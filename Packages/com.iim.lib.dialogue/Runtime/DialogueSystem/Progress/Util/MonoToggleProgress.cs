using System;
using CanvasSystem;
using IIMLib.Core;
using UnityEngine;

namespace DialogueSystem.Progress.Util
{
    public class MonoToggleProgress : IDialogueProgress, IDialogueMonoToggle
    {
        public GameObject GameObject { get; private set; }

        public bool NeedsUpdate => false;
        public Action OnStart { get; set; }
        public Action OnFinish { get; set; }

        private bool CleanState {get; set;}

        public MonoToggleProgress(ICanvasIdentifier identifier)
        {
            if(!ServiceLocator.TryGet(out ICanvasService service)) return;
            GameObject = service.GetCanvas(identifier).gameObject;
        }
        
        public MonoToggleProgress(IUIIdentifier identifier)
        {
            if(!ServiceLocator.TryGet(out ICanvasService service)) return;
            GameObject = service.GetUI(identifier);
        }
        
        public void Start()
        {
            CleanState = GameObject.activeSelf;
            GameObject.SetActive(!CleanState);
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
            GameObject.SetActive(CleanState);
        }
    }
}