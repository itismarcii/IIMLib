using System.Text;
using UnityEngine;

namespace DialogueSystem.Scriptable
{
    [CreateAssetMenu(menuName = "IIM/Dialogue/Text/Nested Logic")]
    public class DialogueNestedLogicScriptable : AbstractDialogueMethodStringScriptable
    {
        [field: SerializeField] public AbstractDialogueMethodStringScriptable[] NestedLogics { get; private set; }

        public override string GetMethodString()
        {
            var builder = new StringBuilder();

            builder.Append(GetTypePrefix(Type));
            builder.Append(Method.Identifier);
            builder.Append('(');
            
            if (NestedLogics != null)
            {
                var first = true;

                foreach (var nestedLogic in NestedLogics)
                {
                    if (!nestedLogic) continue;

                    if (!first) builder.Append(Method.Separator);

                    builder.Append(nestedLogic.GetMethodString());
                    first = false;
                }
            }
            
            builder.Append(')');
            builder.Append(GetTypeSuffix(Type));

            return builder.ToString();
        }
    }
}
