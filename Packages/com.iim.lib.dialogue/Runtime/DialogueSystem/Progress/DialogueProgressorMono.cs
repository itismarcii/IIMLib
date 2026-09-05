using System;
using System.Collections.Generic;
using UnityEngine;

namespace DialogueSystem.Progress
{
    public class DialogueProgressorMono : MonoBehaviour, IDialogueProgressor
    {
        private Queue<IDialogueProgress> _ProgressQueueToDo = new ();
        private Queue<IDialogueProgress> _ProgressQueueHasDone = new ();
        private IDialogueProgress _CurrentProgress;
        
        public bool IsRunning { get; private set; }
        public bool IsFinished { get; private set; }
        public bool IsPaused { get; set; }

        private bool _IsUpdating { get; set; }
        
        public Action OnStart;
        public Action OnProgress;
        public Action OnFinish;
        
        public void Setup(IDialogueProgress[] progresses)
        {
            Clear();
            _ProgressQueueToDo = new Queue<IDialogueProgress>(progresses);
            IsRunning = false;
        }

        public void Start()
        {
            OnStart?.Invoke();
            Progress();
        }

        private void Progress()
        {
            _IsUpdating = false;
            
            if (_CurrentProgress != null)
            {
                _CurrentProgress.OnFinish -= Progress;
                _CurrentProgress.Reset();
                _ProgressQueueHasDone.Enqueue(_CurrentProgress);
            }
            
            _CurrentProgress = _ProgressQueueToDo.Count > 0 ? _ProgressQueueToDo.Dequeue()  : null;

            if (_CurrentProgress == null)
            {
                Finish();
                return;
            }

            if (_CurrentProgress.NeedsUpdate) _IsUpdating = true;

            _CurrentProgress.OnFinish += Progress;
            OnProgress?.Invoke();
            _CurrentProgress.Start();
        }
        
        private void Update()
        {
            if (!_IsUpdating || IsPaused || _CurrentProgress == null) return;
            _CurrentProgress.Update(Time.deltaTime);
        }

        public void Clear()
        {
            _CurrentProgress = null;
            
            _ProgressQueueHasDone.Clear();
            _ProgressQueueToDo.Clear();
            
            IsRunning = false;
            IsFinished = false;
        }

        public void Finish()
        {
            OnFinish?.Invoke();
        }
    }
}