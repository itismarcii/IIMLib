using System;
using UnityEngine;

namespace DialogueSystem.Progress.Util
{
    public class AnimationProgress : IDialogueProgress, IDialogueAnimation
    {
        public Animator Animator { get; private set; }
        public string AnimationName { get; private set; }

        public float Duration { get; private set; }

        public Action OnStart { get; set; }
        public Action OnFinish { get; set; }
        
        public bool NeedsUpdate => true;

        private float _CurrentProgressTime = 0f;
        
        public void Start()
        {
            Animator.SetBool(AnimationName, true);
        }

        public void Finish()
        {
            Animator.SetBool(AnimationName, false);
        }

        public void Update(float deltaTime)
        {
            _CurrentProgressTime += deltaTime;
            if (_CurrentProgressTime >= Duration) Finish();
        }

        public void Reset()
        {
            _CurrentProgressTime = 0f;
        }
    }
}
