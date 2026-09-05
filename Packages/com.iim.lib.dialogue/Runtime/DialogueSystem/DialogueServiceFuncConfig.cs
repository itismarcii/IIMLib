namespace DialogueSystem
{
    public interface IDialogueServiceConfig
    {
        public IDialogueFunc[] Func { get; }
        public IDialogueIdentifierDictionary[] Identifiers { get; }
    }
}