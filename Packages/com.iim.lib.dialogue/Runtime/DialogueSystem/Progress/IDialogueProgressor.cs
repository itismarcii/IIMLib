namespace DialogueSystem.Progress
{
    public interface IDialogueProgressor
    {
        public void Setup(IDialogueProgress[] progresses);
        public void Start();
        public void Clear();
        public void Finish();
    }
}
