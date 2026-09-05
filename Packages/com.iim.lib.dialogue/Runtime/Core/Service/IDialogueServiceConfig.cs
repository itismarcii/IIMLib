using IIMLib.Dialogue.Parsing;

namespace IIMLib.Dialogue.Service
{
    public interface IDialogueServiceConfig
    {
        DialogueSyntax Syntax { get; }
        IDialogueFunction[] Functions { get; }
        IDialogueIdentifierResolver[] IdentifierResolvers { get; }
        int TemplateCacheCapacity { get; }
    }
}
