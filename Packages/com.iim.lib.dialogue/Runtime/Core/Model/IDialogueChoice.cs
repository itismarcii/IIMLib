namespace IIMLib.Dialogue.Model
{
    public interface IDialogueChoice
    {
        IDialogueText Text { get; }
        IDialogueScene Scene { get; }
    }
}
