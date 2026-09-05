using IIMLib.Dialogue.Parsing.BuiltIn;
using UnityEngine;

namespace IIMLib.Dialogue.Authoring
{
    [CreateAssetMenu(menuName = "IIM/Dialogue/Method/Division", fileName = "DivisionMethod")]
    public sealed class DivisionMethodAsset : DialogueMethodAsset
    {
        public override string Identifier => Division.Key;
        public override string Separator => "/";
    }
}
