using System.Collections.Generic;
using DialogueSystem;
using DialogueSystem.Scriptable;
using IIMLib.Core;
using IIMLib.Loop;
using UnityEngine;
using Debug = UnityEngine.Debug;

public class DialogueManager : GameManagerIIMAbstract
{
    [field: SerializeField] public DialogueTextLogicScriptable Dialogue { get; private set; }
    
    private void Start()
    {
        var dictionary = new TestDialogueDictionary();
        ServiceLocator.Get<IDialogueService>().TryAddIdentifier(dictionary);
        
        Debug.Log(Dialogue.Text);
    }
}

public class TestDialogueDictionary : IDialogueIdentifierDictionary
{
    Dictionary<string, string> IDialogueIdentifierDictionary.IdentifierCollection { get; } = new()
    {
        {"User", "Alice"}
    };
}
