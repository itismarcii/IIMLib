using System;
using UnityEngine;

namespace DialogueSystem.Scriptable
{
    [Serializable]
    public struct LocaDialogueParam
    {
        [field: SerializeField] public  ParamType Type { get; private set; }
        [field: SerializeField] public  string Key { get; private set; }
        [field: SerializeField] public  AbstractDialogueMethodStringScriptable Value { get; private set; } }
}