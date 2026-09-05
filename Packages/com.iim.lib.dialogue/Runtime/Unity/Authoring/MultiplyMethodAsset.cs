using IIMLib.Dialogue.Parsing.BuiltIn;
using UnityEngine;

namespace IIMLib.Dialogue.Authoring
{
    [CreateAssetMenu(menuName = "IIM/Dialogue/Method/Multiply", fileName = "MultiplyMethod")]
    public sealed class MultiplyMethodAsset : DialogueMethodAsset
    {
        public override string Identifier => Multiply.Key;
        public override string Separator => "*";
    }
}
