using System;

namespace DialogueSystem.Progress
{
    public interface IDialogueProgress
    {
        public bool NeedsUpdate { get; }
        public Action OnStart { get; set; }
        public Action OnFinish { get; set; }

        public void Start();
        public void Finish();
        public void Update(float deltaTime);
        public void Reset();
    }
}