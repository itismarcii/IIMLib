using IIMLib.Dialogue.Service;

namespace IIMLib.Dialogue.Parsing
{
    public interface IDialogueIdentifierResolver
    {
        bool TryResolve(
            string key,
            DialogueContext context,
            out string value);
    }
}
