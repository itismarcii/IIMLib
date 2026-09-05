namespace DialogueSystem
{
    public interface IDialogueChoice
    {
        public IDialogueText Text { get; }
        public IDialogueScene Scene { get; }
    }
}