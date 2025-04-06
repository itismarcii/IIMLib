using System.IO;
using UnityEditor;
using UnityEditor.AddressableAssets;
using UnityEditor.AddressableAssets.Settings;
using UnityEditor.AddressableAssets.Settings.GroupSchemas;
using UnityEditorInternal;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace IIMLib.Boot.Editor
{
    /// <summary>
    /// Helper methods for CustomBoot configuration
    /// </summary>
    public class BootSettingsUtil
    {
        /// <summary>
        /// Path to the ProjectSettings file
        /// </summary>
        private const string PROJECT_SETTINGS_PATH = "ProjectSettings/Boot.asset";

        /// <summary>
        /// Path to the runtime custom boot settings file
        /// </summary>
        private const string RUNTIME_BOOT_PROJECT_SETTINGS_PATH = "Assets/Boot/Settings/Runtime/BootSettings_Runtime.asset";

        /// <summary>
        /// Path to the editor custom boot settings file
        /// </summary>
        private const string EDITOR_BOOT_PROJECT_SETTINGS_PATH = "Assets/Boot/Settings/Editor/BootSettings_Editor.asset";

        /// <summary>
        /// Determine whether the settings asset file is available
        /// </summary>
        /// <returns></returns>
        internal static bool IsAvailable => File.Exists(PROJECT_SETTINGS_PATH);


        /// <summary>
        /// Retrieve the settings object if it exists, otherwise create and return it.
        /// </summary>
        /// <returns></returns>
        internal static BootSettings GetOrCreateProjectSettings()
        {
            BootSettings settings;

            //Check whether the settings file already exists
            if (IsAvailable)
            {
                //If it exists, load it
                settings = InternalEditorUtility.LoadSerializedFileAndForget(PROJECT_SETTINGS_PATH)[0] as BootSettings;
            }
            else
            {
                //If it doesn't exist, create a new ScriptableObject
                settings = ScriptableObject.CreateInstance<BootSettings>();
                
                //Configure the settings file
                CreateBootSettingsAssets(out var runtimeEntry, out var editorEntry);
                settings.RuntimeSettings = new AssetReference(runtimeEntry.guid);
                settings.EditorSettings = new AssetReference(editorEntry.guid);

                //And save it!
                InternalEditorUtility.SaveToSerializedFileAndForget(new Object[] {settings}, PROJECT_SETTINGS_PATH, true);
            }

            //Finally, return our settings object
            return settings;
        }

        /// <summary>
        /// Create the Runtime and Editor BootProjectSettings assets.
        /// </summary>
        /// <param name="runtimeEntry"></param>
        /// <param name="editorEntry"></param>
        private static void CreateBootSettingsAssets(out AddressableAssetEntry runtimeEntry,
            out AddressableAssetEntry editorEntry)
        {
            //Create two assets representing our boot configurations
            var runtimeSettings = GetOrCreateBootSettingsAsset(RUNTIME_BOOT_PROJECT_SETTINGS_PATH, out var runtimeCreated);
            var editorSettings = GetOrCreateBootSettingsAsset(EDITOR_BOOT_PROJECT_SETTINGS_PATH, out var editorCreated);

            //Save the AssetDatabase state if either asset is new
            if (runtimeCreated || editorCreated) AssetDatabase.SaveAssets();
            
            //Configure the Addressable system with the new assets.
            AddSettingsToAddressable(runtimeSettings, editorSettings, out runtimeEntry, out editorEntry);
        }

        /// <summary>
        /// Load, or create, a BootProjectSettings asset at the given path
        /// </summary>
        /// <param name="path"></param>
        /// <param name="wasCreated"></param>
        /// <returns></returns>
        private static Runtime.BootSettings GetOrCreateBootSettingsAsset(string path, out bool wasCreated)
        {
            var settings = AssetDatabase.LoadAssetAtPath<Runtime.BootSettings>(path);
            wasCreated = false;

            if (settings) return settings;

            //Make sure full path is created
            var dirPath = Path.GetDirectoryName(path);
            
            if (!Directory.Exists(dirPath))
            {
                if (dirPath != null) Directory.CreateDirectory(dirPath);
                AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport);
            }

            settings = ScriptableObject.CreateInstance<Runtime.BootSettings>();
            AssetDatabase.CreateAsset(settings, path);
            wasCreated = true;

            return settings;
        }

        /// <summary>
        /// Add the BootProjectSettings asset to the relevant Addressables groups.
        /// </summary>
        /// <param name="runtimeSettings"></param>
        /// <param name="editorSettings"></param>
        /// <param name="runtimeEntry"></param>
        /// <param name="editorEntry"></param>
        private static void AddSettingsToAddressable(
            Runtime.BootSettings runtimeSettings, Runtime.BootSettings editorSettings, out AddressableAssetEntry runtimeEntry,
            out AddressableAssetEntry editorEntry)
        {
            InitialiseAddressableGroups(out var runtimeGroup, out var editorGroup);
            runtimeEntry = CreateBootProjectSettingsEntry(runtimeSettings, runtimeGroup, $"{nameof(Runtime.BootSettings)}_Runtime");
            editorEntry = CreateBootProjectSettingsEntry(editorSettings, editorGroup, $"{nameof(Runtime.BootSettings)}_Editor");
            AssetDatabase.SaveAssets();
        }

        /// <summary>
        /// Create an Addressables entry for the given BootProjectSettings object, and add it to the given group.
        /// </summary>
        /// <param name="bootSettings"></param>
        /// <param name="group"></param>
        /// <param name="key"></param>
        /// <returns></returns>
        private static AddressableAssetEntry CreateBootProjectSettingsEntry(Runtime.BootSettings bootSettings,
            AddressableAssetGroup group, string key)
        {
            var settings = AddressableAssetSettingsDefaultObject.Settings;
            var entry = settings.CreateOrMoveEntry(AssetDatabase.AssetPathToGUID(AssetDatabase.GetAssetPath(bootSettings)), group);            
            entry.address = key;
            settings.SetDirty(AddressableAssetSettings.ModificationEvent.EntryMoved, entry, true);
            return entry;
        }

        /// <summary>
        /// Ensure the Runtime and Editor Addressables groups exist
        /// </summary>
        /// <param name="runtimeGroup"></param>
        /// <param name="editorGroup"></param>
        private static void InitialiseAddressableGroups(out AddressableAssetGroup runtimeGroup,
            out AddressableAssetGroup editorGroup)
        {
            runtimeGroup = GetOrCreateGroup($"{nameof(Boot)}_Runtime", true);
            editorGroup = GetOrCreateGroup($"{nameof(Boot)}_Editor", false); 
        } 

        /// <summary>
        /// Retrieve or create an Addressables group.
        /// </summary>
        /// <param name="name"></param>
        /// <param name="includeInBuild"></param>
        /// <returns></returns>
        private static AddressableAssetGroup GetOrCreateGroup(string name, bool includeInBuild)
        {
            var settings = AddressableAssetSettingsDefaultObject.Settings;
            var group = settings.FindGroup(name);
            if (group != null) return group;
            
            group = settings.CreateGroup(name, false, false, true, settings.DefaultGroup.Schemas);
            group.GetSchema<BundledAssetGroupSchema>().IncludeInBuild = includeInBuild;

            return group;
        }

        /// <summary>
        /// Retrieve the serialised representation of the settings object
        /// </summary>
        /// <returns></returns>
        internal static SerializedObject GetSerializedSettings() =>  new (GetOrCreateProjectSettings());
    }
}