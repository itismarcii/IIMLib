using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.UIElements;

namespace IIMLib.Boot.Editor
{
    /// <summary>
    /// Settings provider for the custom boot behaviour
    /// </summary>
    public class BootProjectSettingsProvider : SettingsProvider
    {
        private const string SETTINGS_PATH = "Project/Boot " + CORE;
        private const string STYLE_SHEET_PATH = "Packages/com.iim.lib/Editor/Boot/StyleSheets/BootStyles.uss";

        private const string CORE = "Core";
        private const string LOAD = "Load";
        private const string RUNTIME = "Runtime";
        private const string EDITOR = "Editor";

        private const string RUNTIME_SETTINGS = RUNTIME + " " + LOAD;
        private const string EDITOR_SETTINGS = EDITOR + " " + LOAD;
        
        /// <summary>
        /// Internal reference to the serialized boot settings object
        /// </summary>
        private SerializedObject _BootProjectSettings;
        
        private BootProjectSettingsProvider(string path, SettingsScope scopes = SettingsScope.Project, IEnumerable<string> keywords = null) : base(path, scopes, keywords) { }

        /// <summary>
        /// Initialise the UI for the settings provider
        /// </summary>
        /// <param name="searchContext"></param>
        /// <param name="rootElement"></param>
        public override void OnActivate(string searchContext, VisualElement rootElement)
        {
            _BootProjectSettings = BootSettingsUtil.GetSerializedSettings();
            
            rootElement.styleSheets.Add(AssetDatabase.LoadAssetAtPath<StyleSheet>(STYLE_SHEET_PATH));
            rootElement.AddToClassList("settings");
            var title = new Label { text = CORE, style = { fontSize = 28, marginBottom = 20}};
            title.AddToClassList("title");
            rootElement.Add(title);

            var properties = new VisualElement()
            {
                style =
                {
                    flexDirection = FlexDirection.Column,
                },
            };
            
            properties.AddToClassList("property-list");
            properties.Add(SetupPropertiesOfChildren(CreateBootSettingsEditor(_BootProjectSettings.FindProperty(nameof(BootSettings.RuntimeSettings))), RUNTIME_SETTINGS));
            properties.Add(new VisualElement{style = { height = 2, marginTop = 28, marginBottom = 28, backgroundColor = Color.gray, flexGrow = 1, opacity = 0.33f}});
            properties.Add(SetupPropertiesOfChildren(CreateBootSettingsEditor(_BootProjectSettings.FindProperty(nameof(BootSettings.EditorSettings))), EDITOR_SETTINGS));
            rootElement.Add(properties);

            rootElement.Bind(_BootProjectSettings);
        }

        private static VisualElement SetupPropertiesOfChildren(VisualElement rootElement, string titleString)
        {
            var title = rootElement.hierarchy[0];
            ((Label)title).text = titleString;
            title.style.unityFontStyleAndWeight = FontStyle.Bold;
            title.style.fontSize = 13;
            title.style.marginBottom = 8;
            
            return rootElement;
        }
        

        /// <summary>
        /// Draw an editor for the CustomBootSettings object associated with the given property
        /// </summary>
        /// <param name="property"></param>
        /// <returns></returns>
        private static VisualElement CreateBootSettingsEditor(SerializedProperty property)
        {
            var propertyEditorContainer = new VisualElement();
            DrawObject(propertyEditorContainer,
                new SerializedObject(AssetDatabase.LoadAssetAtPath<Runtime.BootSettings>(
                    AssetDatabase.GUIDToAssetPath((property.boxedValue as AssetReference)?.AssetGUID))));

            propertyEditorContainer.style.fontSize = 12;
            return propertyEditorContainer;
        }

        /// <summary>
        /// Generic SerializedObject property editor
        /// </summary>
        /// <param name="container"></param>
        /// <param name="o"></param>
        private static void DrawObject(VisualElement container, SerializedObject o)
        {
            var l = new Label(o.targetObject.name);
            container.Add(l);
            var f = GetVisibleSerializedFields(o.targetObject.GetType());
            foreach (var field in f)
            {
                var prop = o.FindProperty(field.Name);
                var pField = new PropertyField(prop);
                container.Add(pField);
            }
            container.Bind(o);
        }
        
        /// <summary>
        /// Retrieve all accessible serialised fields for the given type
        /// </summary>
        /// <param name="T"></param>
        /// <returns></returns>
        private static FieldInfo[] GetVisibleSerializedFields(Type T)
        {
            var publicFields = T.GetFields(BindingFlags.Instance | BindingFlags.Public);
            var infoFields = publicFields.Where(t => t.GetCustomAttribute<HideInInspector>() == null).ToList();
            
            var privateFields = T.GetFields(BindingFlags.Instance | BindingFlags.NonPublic);
            infoFields.AddRange(privateFields.Where(t => t.GetCustomAttribute<SerializeField>() != null));

            return infoFields.ToArray();
        }

        /// <summary>
        /// Create the Settings Provider. Internally, this will ensure the settings object is created.
        /// </summary>
        /// <returns></returns>
        [SettingsProvider]
        public static SettingsProvider CreateBootSettingsProvider()
        {
            if (!BootSettingsUtil.IsAvailable) BootSettingsUtil.GetOrCreateProjectSettings();
            
            return new BootProjectSettingsProvider(SETTINGS_PATH);
        }
    }
}