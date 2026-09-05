namespace IIMLib.Dialogue.Model
{
    public interface IDialogueScene
    {
        IDialogueSpeaker Speaker { get; }
        IDialogueText Dialogue { get; }
    }
}
