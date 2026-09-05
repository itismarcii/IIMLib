namespace DialogueSystem
{
    public interface IDialogueScene
    {
        public IDialogueText Speaker { get; }
        public IDialogueText Dialogue { get; }
    }
}