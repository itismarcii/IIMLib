using System.IO;
using UnityEditor;
using UnityEditor.AddressableAssets;
using UnityEditor.AddressableAssets.Settings.GroupSchemas;
using UnityEditorInternal;
using UnityEngine;

namespace IIMLib.Boot.Editor
{
    public static class BootSettingsUtil
    {
        public const string SettingsAssetPath = "ProjectSettings/Boot.asset";

        private const string RuntimeDirectory = "Assets/Boot/Settings/Runtime";
        private const string EditorDirectory = "Assets/Boot/Settings/Editor";
        private const string RuntimeAssetPath = RuntimeDirectory + "/BootSettings_Runtime.asset";
        private const string EditorAssetPath = EditorDirectory + "/BootSettings_Editor.asset";

        private const string RuntimeGroupName = "Boot_Runtime";
        private const string EditorGroupName = "Boot_Editor";

        private const string RuntimeAddress = "BootSettings_Runtime";
        private const string EditorAddress = "BootSettings_Editor";

        public static BootSettings GetOrCreateSettings()
        {
            var settings = LoadSettings();

            if (settings == null)
                settings = ScriptableObject.CreateInstance<BootSettings>();

            EnsureBootAssets(settings);
            SaveSettings(settings);
            return settings;
        }

        public static void SaveSettings(BootSettings settings)
        {
            if (settings == null)
                return;

            InternalEditorUtility.SaveToSerializedFileAndForget(
                new UnityEngine.Object[] { settings },
                SettingsAssetPath,
                true);
        }

        private static BootSettings LoadSettings()
        {
            if (!File.Exists(SettingsAssetPath))
                return null;

            var loaded = InternalEditorUtility.LoadSerializedFileAndForget(SettingsAssetPath);
            if (loaded == null)
                return null;

            for (var i = 0; i < loaded.Length; i++)
            {
                if (loaded[i] is BootSettings settings)
                    return settings;
            }

            return null;
        }

        private static void EnsureBootAssets(BootSettings settings)
        {
            EnsureDirectory(RuntimeDirectory);
            EnsureDirectory(EditorDirectory);

            settings.RuntimeSettings = EnsureRuntimeSettingsAsset(
                settings.RuntimeSettings,
                RuntimeAssetPath,
                "BootSettings_Runtime");

            settings.EditorSettings = EnsureRuntimeSettingsAsset(
                settings.EditorSettings,
                EditorAssetPath,
                "BootSettings_Editor");

            EnsureAddressable(settings.RuntimeSettings, RuntimeGroupName, RuntimeAddress, includeInBuild: true);
            EnsureAddressable(settings.EditorSettings, EditorGroupName, EditorAddress, includeInBuild: false);

            EditorUtility.SetDirty(settings);
        }

        private static Runtime.BootSettings EnsureRuntimeSettingsAsset(
            Runtime.BootSettings current,
            string assetPath,
            string assetName)
        {
            if (current != null)
                return current;

            var asset = AssetDatabase.LoadAssetAtPath<Runtime.BootSettings>(assetPath);
            if (asset != null)
                return asset;

            asset = ScriptableObject.CreateInstance<Runtime.BootSettings>();
            asset.name = assetName;
            AssetDatabase.CreateAsset(asset, assetPath);
            AssetDatabase.SaveAssets();
            return asset;
        }

        private static void EnsureAddressable(
            UnityEngine.Object asset,
            string groupName,
            string address,
            bool includeInBuild)
        {
            if (asset == null)
                return;

            var addressableSettings = AddressableAssetSettingsDefaultObject.GetSettings(true);
            if (addressableSettings == null)
            {
                Debug.LogError("Addressables settings could not be created.");
                return;
            }

            var group = addressableSettings.FindGroup(groupName);
            if (group == null)
            {
                group = addressableSettings.CreateGroup(
                    groupName,
                    false,
                    false,
                    true,
                    null,
                    typeof(ContentUpdateGroupSchema),
                    typeof(BundledAssetGroupSchema));
            }

            var bundledSchema = group.GetSchema<BundledAssetGroupSchema>();
            if (bundledSchema == null)
                bundledSchema = group.AddSchema<BundledAssetGroupSchema>();

            bundledSchema.IncludeInBuild = includeInBuild;

            var guid = AssetDatabase.AssetPathToGUID(AssetDatabase.GetAssetPath(asset));
            var entry = addressableSettings.CreateOrMoveEntry(guid, group);
            entry.address = address;

            EditorUtility.SetDirty(bundledSchema);
            EditorUtility.SetDirty(group);
            EditorUtility.SetDirty(addressableSettings);
            AssetDatabase.SaveAssets();
        }

        private static void EnsureDirectory(string path)
        {
            if (AssetDatabase.IsValidFolder(path))
                return;

            var parts = path.Split('/');
            var current = parts[0];

            for (var i = 1; i < parts.Length; i++)
            {
                var next = $"{current}/{parts[i]}";
                if (!AssetDatabase.IsValidFolder(next))
                    AssetDatabase.CreateFolder(current, parts[i]);

                current = next;
            }
        }
    }
}
