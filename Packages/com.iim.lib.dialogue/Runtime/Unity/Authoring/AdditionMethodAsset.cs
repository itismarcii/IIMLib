using IIMLib.Dialogue.Parsing.BuiltIn;
using UnityEngine;

namespace IIMLib.Dialogue.Authoring
{
    [CreateAssetMenu(menuName = "IIM/Dialogue/Method/Addition", fileName = "AdditionMethod")]
    public sealed class AdditionMethodAsset : DialogueMethodAsset
    {
        public override string Identifier => Addition.Key;
        public override string Separator => "+";
    }
}
