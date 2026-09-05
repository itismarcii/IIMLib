using System;
using System.Text;
using UnityEngine;

namespace DialogueSystem.Scriptable
{
    [CreateAssetMenu(menuName = "IIM/Dialogue/Logic")]
    public class DialogueTextLogicScriptable : AbstractDialogueTextScriptable
    {
        [Serializable]
        public struct Container
        {
            [field: SerializeField] public string Key { get; private set; } 
            [field: SerializeField] public AbstractDialogueMethodStringScriptable[] Params { get; private set; } 
        }
        
        [field: SerializeField] public string Prefix { get; private set; } 
        [field: SerializeField] public string Suffix { get; private set; } 

        [field: SerializeField, TextArea] public string String { get; private set; } 
        [field: SerializeField] public Container[] Params { get; private set; } 
        
        protected override string GetTextString()
        {
            if (string.IsNullOrEmpty(String)) return string.Empty;

            var result = String;

            if (Params == null || Params.Length == 0) return result;

            var prefix = Prefix ?? string.Empty;
            var suffix = Suffix ?? string.Empty;

            foreach (var container in Params)
            {
                if (string.IsNullOrEmpty(container.Key))
                    continue;

                var token = prefix + container.Key + suffix;
                var replacement = ResolveContainerValue(container);

                result = result.Replace(token, replacement);
            }

            return result;
        }

        private static string ResolveContainerValue(Container container)
        {
            if (container.Params == null || container.Params.Length == 0) return string.Empty;

            var sb = new StringBuilder();

            foreach (var method in container.Params)
            {
                if (method == null) continue;

                var value = method.GetMethodString();

                if (!string.IsNullOrEmpty(value)) sb.Append(value);
            }

            return sb.ToString();
        }
    }
}