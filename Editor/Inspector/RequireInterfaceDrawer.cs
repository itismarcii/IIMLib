using UnityEditor;
using UnityEngine;

namespace IIMLib.Core.Editor
{
    [CustomPropertyDrawer(typeof(RequireInterfaceAttribute))]
    public sealed class RequireInterfaceDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            var required = ((RequireInterfaceAttribute)attribute).RequiredType;

            if (property.propertyType != SerializedPropertyType.ObjectReference)
            {
                EditorGUI.HelpBox(position, $"{nameof(RequireInterfaceAttribute)} can only be used on object references.", MessageType.Error);
                return;
            }

            EditorGUI.BeginProperty(position, label, property);

            var current = property.objectReferenceValue;
            var next = EditorGUI.ObjectField(position, label, current, typeof(UnityEngine.Object), true);

            if (next == null)
            {
                property.objectReferenceValue = null;
            }
            else if (TryResolveRequiredObject(next, required, out var resolved))
            {
                property.objectReferenceValue = resolved;
            }
            else
            {
                Debug.LogWarning($"Assigned object '{next.name}' does not implement '{required.FullName}'.");
            }

            EditorGUI.EndProperty();
        }

        private static bool TryResolveRequiredObject(UnityEngine.Object candidate, System.Type requiredType, out UnityEngine.Object resolved)
        {
            if (requiredType.IsInstanceOfType(candidate))
            {
                resolved = candidate;
                return true;
            }

            if (candidate is GameObject gameObject)
            {
                var components = gameObject.GetComponents<Component>();
                for (var i = 0; i < components.Length; i++)
                {
                    if (components[i] != null && requiredType.IsInstanceOfType(components[i]))
                    {
                        resolved = components[i];
                        return true;
                    }
                }
            }

            if (candidate is Component component)
            {
                var components = component.GetComponents<Component>();
                for (var i = 0; i < components.Length; i++)
                {
                    if (components[i] != null && requiredType.IsInstanceOfType(components[i]))
                    {
                        resolved = components[i];
                        return true;
                    }
                }
            }

            resolved = null;
            return false;
        }
    }
}
