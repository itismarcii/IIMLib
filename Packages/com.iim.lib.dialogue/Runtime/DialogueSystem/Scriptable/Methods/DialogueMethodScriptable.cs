using UnityEngine;

namespace DialogueSystem.Scriptable.Methods
{
    public abstract class DialogueMethodScriptable : ScriptableObject
    {
        public abstract string Identifier { get; } 
		public abstract string Separator { get; }
    }
}