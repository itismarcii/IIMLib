using System;
using System.Collections.Generic;
using System.Text;
using IIMLib.Dialogue.Parsing;
using UnityEngine;

namespace IIMLib.Dialogue.Authoring
{
    [CreateAssetMenu(menuName = "IIM/Dialogue/Nested Method", fileName = "DialogueNestedMethod")]
    public sealed class DialogueNestedMethodAsset : AbstractDialogueMethodStringAsset
    {
        [SerializeField] private AbstractDialogueMethodStringAsset[] _nestedMethods;

        public IReadOnlyList<AbstractDialogueMethodStringAsset> NestedMethods =>
            _nestedMethods ?? Array.Empty<AbstractDialogueMethodStringAsset>();

        protected override bool AppendMethodString(
            DialogueSyntax syntax,
            StringBuilder builder,
            HashSet<AbstractDialogueMethodStringAsset> active)
        {
            if (Type == DialogueParameterType.Function &&
                (Method == null || string.IsNullOrWhiteSpace(Method.Identifier)))
                return false;

            var prefix = GetTypePrefix(Type, syntax);
            var suffix = GetTypeSuffix(Type, syntax);

            builder.Append(prefix);

            if (Type == DialogueParameterType.Function)
            {
                builder.Append(Method.Identifier.Trim());
                builder.Append('(');
            }

            var separator = Method != null ? Method.Separator : string.Empty;
            var first = true;

            if (_nestedMethods != null)
            {
                for (var i = 0; i < _nestedMethods.Length; i++)
                {
                    var nested = _nestedMethods[i];
                    if (nested == null)
                        continue;

                    if (!first && !string.IsNullOrEmpty(separator))
                        builder.Append(separator);

                    var before = builder.Length;
                    if (!nested.TryAppendMethodString(syntax, builder, active))
                        return false;

                    if (builder.Length == before)
                        continue;

                    first = false;
                }
            }

            if (Type == DialogueParameterType.Identifier && first)
                return false;

            if (Type == DialogueParameterType.Function)
                builder.Append(')');

            builder.Append(suffix);
            return true;
        }

        protected override long CombineContentVersion(
            long localVersion,
            HashSet<AbstractDialogueMethodStringAsset> active)
        {
#if UNITY_EDITOR
            var baseVersion = base.CombineContentVersion(localVersion, active);
            if (_nestedMethods == null)
                return baseVersion;

            unchecked
            {
                var version = baseVersion;
                for (var i = 0; i < _nestedMethods.Length; i++)
                {
                    var nested = _nestedMethods[i];
                    if (nested != null)
                        version = (version * 397) ^ nested.GetContentVersion(active);
                }

                return version;
            }
#else
            return base.CombineContentVersion(localVersion, active);
#endif
        }
    }
}
