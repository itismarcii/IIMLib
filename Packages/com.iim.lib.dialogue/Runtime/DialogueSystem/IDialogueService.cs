using IIMLib.Core;

namespace DialogueSystem
{
    public interface IDialogueService : IService
    {
        public string FuncPrefix { get; }
        public string FuncSuffix { get; }
        
        public string IdentifierPrefix { get; }
        public string IdentifierSuffix { get; }
        
        public bool TryAddFunction(string key, IDialogueFunc func);
        public bool TryAddIdentifier(IDialogueIdentifierDictionary identifierDictionary);
        public string GetFunctionString(string identifier, string key = null);
        public bool IsFunctionString(string s);
        public string ResolveDialogueString(string s);
    }
}