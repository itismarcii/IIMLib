using System;
using System.Collections.Generic;
using System.Text;
using IIMLib.Dialogue.Parsing;
using UnityEngine;

namespace IIMLib.Dialogue.Authoring
{
    public abstract class AbstractDialogueMethodStringAsset : ScriptableObject
    {
        [SerializeField] private DialogueParameterType _type;
        [SerializeField] private DialogueMethodAsset _method;

        [NonSerialized] private long _contentVersion;

        [ThreadStatic] private static StringBuilder _sharedBuilder;
        [ThreadStatic] private static HashSet<AbstractDialogueMethodStringAsset> _sharedActive;
        [ThreadStatic] private static HashSet<AbstractDialogueMethodStringAsset> _sharedVersionActive;
        [ThreadStatic] private static int _methodStringDepth;
        [ThreadStatic] private static int _contentVersionDepth;

        public DialogueParameterType Type => _type;
        public DialogueMethodAsset Method => _method;

        public string GetMethodString() => GetMethodString(DialogueSyntax.Default);

        public string GetMethodString(DialogueSyntax syntax)
        {
            return TryGetMethodString(syntax, out var value)
                ? value
                : string.Empty;
        }

        public bool TryGetMethodString(DialogueSyntax syntax, out string value)
        {
            syntax ??= DialogueSyntax.Default;

            var useShared = _methodStringDepth == 0;
            var builder = useShared
                ? (_sharedBuilder ??= new StringBuilder(64))
                : new StringBuilder(64);
            var active = useShared
                ? (_sharedActive ??= new HashSet<AbstractDialogueMethodStringAsset>())
                : new HashSet<AbstractDialogueMethodStringAsset>();

            _methodStringDepth++;
            try
            {
                builder.Clear();
                active.Clear();

                if (!TryAppendMethodString(syntax, builder, active))
                {
                    value = string.Empty;
                    return false;
                }

                value = builder.ToString();
                return true;
            }
            finally
            {
                builder.Clear();
                active.Clear();
                _methodStringDepth--;
            }
        }

        public long GetContentVersion()
        {
            var useShared = _contentVersionDepth == 0;
            var active = useShared
                ? (_sharedVersionActive ??=
                    new HashSet<AbstractDialogueMethodStringAsset>())
                : new HashSet<AbstractDialogueMethodStringAsset>();

            _contentVersionDepth++;
            try
            {
                active.Clear();
                return GetContentVersion(active);
            }
            finally
            {
                active.Clear();
                _contentVersionDepth--;
            }
        }

        internal long GetContentVersion(
            HashSet<AbstractDialogueMethodStringAsset> active)
        {
            if (!active.Add(this))
                return 0;

            if (_contentVersion == 0)
                _contentVersion = DialogueAuthoringRevision.Next();

            var version = CombineContentVersion(_contentVersion, active);
            active.Remove(this);
            return version;
        }

        protected virtual long CombineContentVersion(
            long localVersion,
            HashSet<AbstractDialogueMethodStringAsset> active)
        {
            if (Method == null)
                return localVersion;

            unchecked
            {
                return (localVersion * 397) ^ Method.GetContentVersion();
            }
        }

        internal bool TryAppendMethodString(
            DialogueSyntax syntax,
            StringBuilder builder,
            HashSet<AbstractDialogueMethodStringAsset> active)
        {
            if (!active.Add(this))
                return false;

            var success = AppendMethodString(syntax, builder, active);
            active.Remove(this);
            return success;
        }

        protected abstract bool AppendMethodString(
            DialogueSyntax syntax,
            StringBuilder builder,
            HashSet<AbstractDialogueMethodStringAsset> active);

        protected static string GetTypePrefix(
            DialogueParameterType type,
            DialogueSyntax syntax)
        {
            return type switch
            {
                DialogueParameterType.Function => syntax.FunctionPrefix,
                DialogueParameterType.Identifier => syntax.IdentifierPrefix,
                _ => throw new ArgumentOutOfRangeException(nameof(type), type, null)
            };
        }

        protected static string GetTypeSuffix(
            DialogueParameterType type,
            DialogueSyntax syntax)
        {
            return type switch
            {
                DialogueParameterType.Function => syntax.FunctionSuffix,
                DialogueParameterType.Identifier => syntax.IdentifierSuffix,
                _ => throw new ArgumentOutOfRangeException(nameof(type), type, null)
            };
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
