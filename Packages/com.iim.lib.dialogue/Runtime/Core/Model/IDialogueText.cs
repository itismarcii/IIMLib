using IIMLib.Dialogue.Parsing;

namespace IIMLib.Dialogue.Model
{
    public interface IDialogueText
    {
        string Text { get; }
        string GetText(DialogueSyntax syntax);
    }
}
