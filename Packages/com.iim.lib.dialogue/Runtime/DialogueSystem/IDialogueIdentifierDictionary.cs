using System.Collections.Generic;

namespace DialogueSystem
{
    public interface IDialogueIdentifierDictionary
    {
        protected Dictionary<string, string> IdentifierCollection { get; }

        public bool IsIdentifier(string key) => IdentifierCollection.ContainsKey(key);
        public string Resolve(string key) => IdentifierCollection.GetValueOrDefault(key, key);
    }
}