using UnityEngine;
using UnityEngine.AddressableAssets;

namespace IIMLib.Boot.Editor
{
    /// <summary>
    /// A scriptable object used to store references to CustomBoot settings for both Runtime and Editor.
    /// </summary>
    public class BootSettings : ScriptableObject
    {
        /// <summary>
        /// The Addressables reference for the runtime only settings
        /// </summary>
        public AssetReference RuntimeSettings;
        
        /// <summary>
        /// The Addressables reference for the editor only settings
        /// </summary>
        public AssetReference EditorSettings;
    }
}