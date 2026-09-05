using System.Collections.Generic;
using System.Text;
using IIMLib.Dialogue.Parsing;
using UnityEngine;

namespace IIMLib.Dialogue.Authoring
{
    [CreateAssetMenu(menuName = "IIM/Dialogue/Method String", fileName = "DialogueMethodString")]
    public sealed class DialogueMethodStringAsset : AbstractDialogueMethodStringAsset
    {
        [SerializeField] private string _value;

        protected override bool AppendMethodString(
            DialogueSyntax syntax,
            StringBuilder builder,
            HashSet<AbstractDialogueMethodStringAsset> active)
        {
            if (Type == DialogueParameterType.Identifier)
            {
                if (string.IsNullOrWhiteSpace(_value))
                    return false;

                builder.Append(syntax.IdentifierPrefix);
                builder.Append(_value.Trim());
                builder.Append(syntax.IdentifierSuffix);
                return true;
            }

            if (Method == null || string.IsNullOrWhiteSpace(Method.Identifier))
                return false;

            builder.Append(syntax.FunctionPrefix);
            builder.Append(Method.Identifier.Trim());
            builder.Append('(');
            builder.Append(_value ?? string.Empty);
            builder.Append(')');
            builder.Append(syntax.FunctionSuffix);
            return true;
        }
    }
}
