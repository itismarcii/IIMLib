namespace DialogueSystem
{
    public interface IDialogueFunc
    {
        public string Identifier { get; }
        public string CollectInfo(string s = null);
    }
}