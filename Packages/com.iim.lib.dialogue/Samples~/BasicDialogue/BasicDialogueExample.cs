using System.Collections.Generic;
using IIMLib.Dialogue.Parsing;
using IIMLib.Dialogue.Service;
using UnityEngine;

namespace IIMLib.Dialogue.Sample
{
    public sealed class BasicDialogueExample : MonoBehaviour
    {
        private void Start()
        {
            var dialogue = new DialogueService();
            dialogue.Initialize();
            dialogue.TryAddIdentifier(new ExampleIdentifiers());

            Debug.Log(dialogue.ResolveDialogueString(
                "Hello ={player}. Two plus three is #%{Addition(2+3)}."));
        }

        private sealed class ExampleIdentifiers : IDialogueIdentifierDictionary
        {
            private readonly Dictionary<string, string> _values = new()
            {
                ["player"] = "Player"
            };

            public bool TryResolve(string key, out string value) =>
                _values.TryGetValue(key, out value);
        }
    }
}
