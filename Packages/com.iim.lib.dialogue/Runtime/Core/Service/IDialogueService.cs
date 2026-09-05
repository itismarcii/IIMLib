using IIMLib.Core;
using IIMLib.Dialogue.Model;
using IIMLib.Dialogue.Parsing;

namespace IIMLib.Dialogue.Service
{
    public interface IDialogueService : IService
    {
        DialogueSyntax Syntax { get; }

        DialogueTemplate Compile(string source);
        DialogueTemplate CompileCached(string source);
        void ClearTemplateCache();

        string Resolve(string source, DialogueContext context = null);
        string Resolve(IDialogueText text, DialogueContext context = null);
        string Resolve(DialogueTemplate template, DialogueContext context = null);

        bool TryResolve(
            string source,
            DialogueContext context,
            out string value,
            out DialogueFailure failure);

        bool TryResolve(
            DialogueTemplate template,
            DialogueContext context,
            out string value,
            out DialogueFailure failure);

        DialogueRegistration RegisterFunction(IDialogueFunction function);
        DialogueRegistration RegisterIdentifierResolver(IDialogueIdentifierResolver resolver);

        bool TryEvaluateFunction(
            string identifier,
            string argument,
            DialogueContext context,
            out string value,
            out DialogueFailureReason failureReason);

        bool TryResolveIdentifier(
            string key,
            DialogueContext context,
            out string value);

        bool IsFunctionToken(string value);
        bool IsIdentifierToken(string value);
    }
}
