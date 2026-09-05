using UnityEngine;

namespace IIMLib.Boot.Editor
{
    public sealed class BootSettings : ScriptableObject
    {
        [SerializeField] private Runtime.BootSettings _runtimeSettings;
        [SerializeField] private Runtime.BootSettings _editorSettings;

        public Runtime.BootSettings RuntimeSettings
        {
            get => _runtimeSettings;
            set => _runtimeSettings = value;
        }

        public Runtime.BootSettings EditorSettings
        {
            get => _editorSettings;
            set => _editorSettings = value;
        }
    }
}
