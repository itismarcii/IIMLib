using System;
using System.Collections.Generic;
using IIMLib.Core;
using Loca;
using UnityEngine;

namespace DialogueSystem.Scriptable
{
    [CreateAssetMenu(menuName = "IIM/Dialogue/Loca")]
    public class LocaDialogueTextScriptable: AbstractDialogueTextScriptable
    {
        [field: SerializeField] public string Key { get; private set; }
        [field: SerializeField] public LocaDialogueParam[] Param { get; private set; }

        public override string Text => GetTextString();

        protected override string GetTextString() => !string.IsNullOrEmpty(Key)
            ? ServiceLocator.Get<ILocaService>().Get(Key, FunctionTuple, IdentifierTuple)
            : string.Empty;
       
        private (string, string)[] _FuncTuple;
        private (string, string)[] _IdentifierTuple;

        private (string, string)[] FunctionTuple
        {
            get
            {
                if(_FuncTuple != null) return _FuncTuple;
                
                CreateValueTuples(Param, out var functions, out var identifiers);

                _IdentifierTuple ??= identifiers.ToArray();
                _FuncTuple = functions.ToArray();

                return _FuncTuple;
            }
        }
        
        private (string, string)[] IdentifierTuple
        {
            get
            {
                if(_IdentifierTuple != null) return _IdentifierTuple;
                
                CreateValueTuples(Param, out var functions, out var identifiers);
                
                _IdentifierTuple = identifiers.ToArray();
                _FuncTuple ??= functions.ToArray();

                return _IdentifierTuple;
            }
        }
        
        private static void CreateValueTuples(LocaDialogueParam[] paramArray, out List<(string, string)> functions, out List<(string, string)> identifiers)
        {
            functions = new List<(string, string)>();
            identifiers = new List<(string, string)>();

            foreach (var param in paramArray)
            {
                if (string.IsNullOrEmpty(param.Key) || param.Value is not {} provider) continue;
                
                switch (param.Type)
                {
                    case ParamType.Function:
                        functions.Add((param.Key, DialogueService.ResolveDialogueString($"{provider.GetMethodString()}")));
                        break;
                    case ParamType.Identifier:
                        identifiers.Add((param.Key, DialogueService.ResolveDialogueString($"{provider.GetMethodString()}")));
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
        }
    }
}
