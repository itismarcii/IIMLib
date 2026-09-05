using System;
using IIMLib.Core;
using UnityEditor;
using UnityEngine;

namespace IIMLib.Core.Editor
{
    [CustomPropertyDrawer(typeof(RequireInterfaceAttribute))]
    public sealed class RequireInterfaceDrawer : PropertyDrawer
    {
        private const float Spacing = 2f;

        public override void OnGUI(
            Rect position,
            SerializedProperty property,
            GUIContent label)
        {
            var requiredType =
                ((RequireInterfaceAttribute)attribute).RequiredType;

            EditorGUI.BeginProperty(position, label, property);

            if (property.isArray)
            {
                DrawArray(position, property, label, requiredType);
            }
            else
            {
                DrawObjectReference(
                    position,
                    property,
                    label,
                    requiredType);
            }

            EditorGUI.EndProperty();
        }

        public override float GetPropertyHeight(
            SerializedProperty property,
            GUIContent label)
        {
            if (!property.isArray || !property.isExpanded)
                return EditorGUIUtility.singleLineHeight;

            var lineHeight = EditorGUIUtility.singleLineHeight;

            return lineHeight
                   + Spacing
                   + lineHeight
                   + property.arraySize * (lineHeight + Spacing);
        }

        private static void DrawArray(
            Rect position,
            SerializedProperty property,
            GUIContent label,
            Type requiredType)
        {
            var lineHeight = EditorGUIUtility.singleLineHeight;

            var foldoutRect = new Rect(
                position.x,
                position.y,
                position.width,
                lineHeight);

            property.isExpanded = EditorGUI.Foldout(
                foldoutRect,
                property.isExpanded,
                label,
                true);

            if (!property.isExpanded)
                return;

            EditorGUI.indentLevel++;

            var y = foldoutRect.yMax + Spacing;

            var sizeRect = new Rect(
                position.x,
                y,
                position.width,
                lineHeight);

            var newSize = EditorGUI.IntField(
                sizeRect,
                "Size",
                property.arraySize);

            newSize = Mathf.Max(0, newSize);

            if (newSize != property.arraySize)
                ResizeArray(property, newSize);

            y += lineHeight + Spacing;

            for (var i = 0; i < property.arraySize; i++)
            {
                var element = property.GetArrayElementAtIndex(i);

                var elementRect = new Rect(
                    position.x,
                    y,
                    position.width,
                    lineHeight);

                DrawObjectReference(
                    elementRect,
                    element,
                    new GUIContent($"Element {i}"),
                    requiredType);

                y += lineHeight + Spacing;
            }

            EditorGUI.indentLevel--;
        }

        private static void DrawObjectReference(
            Rect position,
            SerializedProperty property,
            GUIContent label,
            Type requiredType)
        {
            if (property.propertyType != SerializedPropertyType.ObjectReference)
            {
                EditorGUI.HelpBox(
                    position,
                    $"{nameof(RequireInterfaceAttribute)} " +
                    "can only be used with UnityEngine.Object references.",
                    MessageType.Error);

                return;
            }

            var current = property.objectReferenceValue;

            var candidate = EditorGUI.ObjectField(
                position,
                label,
                current,
                typeof(UnityEngine.Object),
                true);

            if (candidate == current)
                return;

            if (candidate == null)
            {
                property.objectReferenceValue = null;
                return;
            }

            if (TryResolveRequiredObject(
                    candidate,
                    requiredType,
                    out var resolved))
            {
                property.objectReferenceValue = resolved;
                return;
            }

            Debug.LogWarning(
                $"'{candidate.name}' does not implement " +
                $"'{requiredType.FullName}'.");
        }

        private static bool TryResolveRequiredObject(
            UnityEngine.Object candidate,
            Type requiredType,
            out UnityEngine.Object resolved)
        {
            if (requiredType.IsInstanceOfType(candidate))
            {
                resolved = candidate;
                return true;
            }

            if (candidate is GameObject gameObject)
            {
                return TryFindComponent(
                    gameObject,
                    requiredType,
                    out resolved);
            }

            if (candidate is Component component)
            {
                return TryFindComponent(
                    component.gameObject,
                    requiredType,
                    out resolved);
            }

            resolved = null;
            return false;
        }

        private static bool TryFindComponent(
            GameObject gameObject,
            Type requiredType,
            out UnityEngine.Object resolved)
        {
            var components = gameObject.GetComponents<Component>();

            for (var i = 0; i < components.Length; i++)
            {
                var component = components[i];

                if (component == null)
                    continue;

                if (!requiredType.IsInstanceOfType(component))
                    continue;

                resolved = component;
                return true;
            }

            resolved = null;
            return false;
        }

        private static void ResizeArray(
            SerializedProperty property,
            int newSize)
        {
            var previousSize = property.arraySize;

            property.arraySize = newSize;

            if (newSize <= previousSize)
                return;

            for (var i = previousSize; i < newSize; i++)
            {
                var element = property.GetArrayElementAtIndex(i);

                if (element.propertyType ==
                    SerializedPropertyType.ObjectReference)
                {
                    element.objectReferenceValue = null;
                }
            }
        }
    }
}