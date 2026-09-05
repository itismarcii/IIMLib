using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine.UIElements;

namespace IIMLib.Boot.Editor
{
    public sealed class BootProjectSettingsProvider : SettingsProvider
    {
        private const string SettingsPath = "Project/Boot Core";
        private const string StyleSheetPath = "Packages/com.iim.lib/Editor/Boot/StyleSheets/BootStyles.uss";

        private SerializedObject _serializedSettings;

        private BootProjectSettingsProvider(string path, SettingsScope scope)
            : base(path, scope)
        {
        }

        [SettingsProvider]
        public static SettingsProvider CreateProvider()
        {
            return new BootProjectSettingsProvider(SettingsPath, SettingsScope.Project)
            {
                keywords = new[] { "boot", "startup", "addressables", "iim" }
            };
        }

        public override void OnActivate(string searchContext, VisualElement rootElement)
        {
            var settings = BootSettingsUtil.GetOrCreateSettings();
            _serializedSettings = new SerializedObject(settings);

            var styleSheet = AssetDatabase.LoadAssetAtPath<StyleSheet>(StyleSheetPath);
            if (styleSheet != null)
                rootElement.styleSheets.Add(styleSheet);

            var container = new VisualElement();
            container.AddToClassList("settings");

            var title = new Label("Boot Core");
            title.AddToClassList("title");
            container.Add(title);

            var description = new HelpBox(
                "Runtime boot objects are included in player builds. Editor boot objects are only loaded while running in the Unity Editor.",
                HelpBoxMessageType.Info);
            container.Add(description);

            var runtimeField = new PropertyField(_serializedSettings.FindProperty("_runtimeSettings"), "Runtime Settings");
            var editorField = new PropertyField(_serializedSettings.FindProperty("_editorSettings"), "Editor Settings");

            runtimeField.Bind(_serializedSettings);
            editorField.Bind(_serializedSettings);

            container.Add(runtimeField);
            container.Add(editorField);

            var ensureButton = new Button(() =>
            {
                BootSettingsUtil.GetOrCreateSettings();
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
            })
            {
                text = "Ensure Boot Assets & Addressables"
            };

            container.Add(ensureButton);
            rootElement.Add(container);
        }

        public override void OnDeactivate()
        {
            if (_serializedSettings?.targetObject is BootSettings settings)
            {
                _serializedSettings.ApplyModifiedProperties();
                BootSettingsUtil.SaveSettings(settings);
            }

            _serializedSettings = null;
        }
    }
}
