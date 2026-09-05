using System.Collections.Generic;
using IIMLib.Dialogue.Authoring;
using UnityEditor;
using UnityEngine;

namespace IIMLib.Dialogue.Editor
{
    [CustomEditor(typeof(DialogueNestedMethodAsset))]
    internal sealed class DialogueNestedMethodAssetEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();

            var asset = (DialogueNestedMethodAsset)target;
            if (!DialogueAssetCycleValidator.TryFindCycle(asset, out var cycle))
                return;

            EditorGUILayout.Space();
            EditorGUILayout.HelpBox(
                $"Nested dialogue method cycle detected: {cycle}",
                MessageType.Error);
        }
    }

    internal static class DialogueAssetCycleValidator
    {
        public static bool TryFindCycle(
            DialogueNestedMethodAsset root,
            out string cycle)
        {
            var active = new HashSet<AbstractDialogueMethodStringAsset>();
            var path = new List<AbstractDialogueMethodStringAsset>();

            if (Visit(root, active, path, out var cycleStart))
            {
                var builder = new System.Text.StringBuilder();
                for (var i = cycleStart; i < path.Count; i++)
                {
                    if (builder.Length > 0)
                        builder.Append(" -> ");

                    builder.Append(path[i] != null ? path[i].name : "<null>");
                }

                builder.Append(" -> ").Append(path[cycleStart].name);
                cycle = builder.ToString();
                return true;
            }

            cycle = null;
            return false;
        }

        private static bool Visit(
            AbstractDialogueMethodStringAsset asset,
            HashSet<AbstractDialogueMethodStringAsset> active,
            List<AbstractDialogueMethodStringAsset> path,
            out int cycleStart)
        {
            cycleStart = -1;

            if (asset == null)
                return false;

            if (active.Contains(asset))
            {
                cycleStart = path.IndexOf(asset);
                return cycleStart >= 0;
            }

            if (!(asset is DialogueNestedMethodAsset nested))
                return false;

            active.Add(asset);
            path.Add(asset);

            var children = nested.NestedMethods;
            for (var i = 0; i < children.Count; i++)
            {
                if (Visit(children[i], active, path, out cycleStart))
                    return true;
            }

            active.Remove(asset);
            path.RemoveAt(path.Count - 1);
            return false;
        }
    }
}
