using System;
using UnityEngine;

namespace IIMLib.Dialogue.Authoring
{
    public abstract class DialogueMethodAsset : ScriptableObject
    {
        [NonSerialized] private long _contentVersion;

        public abstract string Identifier { get; }
        public abstract string Separator { get; }

        public long GetContentVersion()
        {
            if (_contentVersion == 0)
                _contentVersion = DialogueAuthoringRevision.Next();

            return _contentVersion;
        }

        protected void MarkContentDirty()
        {
            _contentVersion = DialogueAuthoringRevision.Next();
        }

#if UNITY_EDITOR
        protected virtual void OnValidate()
        {
            MarkContentDirty();
        }
#endif
    }
}
