using System;
using DialogueSystem.Scriptable.Methods;
using IIMLib.Core;
using UnityEngine;

namespace DialogueSystem.Scriptable
{
    public abstract class AbstractDialogueMethodStringScriptable : ScriptableObject
    {
        [field: SerializeField] public ParamType Type { get; private set; }
        [field: SerializeField] public DialogueMethodScriptable Method { get; private set; }

        public abstract string GetMethodString();
        
        protected static string GetTypePrefix(ParamType paramType)
        {
            return paramType switch
            {
                ParamType.Function => ServiceLocator.Get<IDialogueService>().FuncPrefix,
                ParamType.Identifier => ServiceLocator.Get<IDialogueService>().IdentifierPrefix,
                _ => throw new ArgumentOutOfRangeException(nameof(paramType), paramType, null)
            };
        }
        
        protected static string GetTypeSuffix(ParamType paramType)
        {
            return paramType switch
            {
                ParamType.Function => ServiceLocator.Get<IDialogueService>().FuncSuffix,
                ParamType.Identifier => ServiceLocator.Get<IDialogueService>().IdentifierSuffix,
                _ => throw new ArgumentOutOfRangeException(nameof(paramType), paramType, null)
            };
        }
    }
}