using System;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace IIMLib.Boot.Runtime
{
    public static class Boot
    {
        public static bool Initialized { get; private set; }

        private static AsyncOperationHandle<BootSettings> _runtimeBootSettingsHandle;
        private static AsyncOperationHandle<BootSettings> _editorBootSettingsHandle;

        private static readonly string RuntimeAsset = $"{nameof(Runtime.BootSettings)}_Runtime";
        private static readonly string EditorAsset = $"{nameof(BootSettings)}_Editor";
        
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSplashScreen)]
        private static void Initialize()
        {
            Application.quitting += ApplicationUnload;

            Init();
        }

        private static void Init()
        {
            if (Application.isEditor)
                InitializeBootSettingSync();
            else
                _ = InitializeBootSettings();
        }

        private static void ApplicationUnload()
        {
            Application.quitting -= ApplicationUnload;
            DeInit();
        }

        private static void DeInit()
        {
            Cleanup(_runtimeBootSettingsHandle);
            Cleanup(_editorBootSettingsHandle);
            Initialized = false;
        }

        private static async Task InitializeBootSettings()
        {
            await LoadBootSettings();
            Initialized = true;
        }

        private static void InitializeBootSettingSync()
        {
            LoadBootSettingsSync();
            Initialized = true;
        }

        private static async Task LoadBootSettings()
        {
            if (Application.isEditor)
            {
                _editorBootSettingsHandle = await InitialiseBootSettingsAsset(EditorAsset);
            }
            
            _runtimeBootSettingsHandle = await InitialiseBootSettingsAsset(RuntimeAsset);
        }

        private static void LoadBootSettingsSync()
        {
            if (Application.isEditor) _editorBootSettingsHandle = InitialiseBootSettingsAssetSync(EditorAsset);

            _runtimeBootSettingsHandle = InitialiseBootSettingsAssetSync(RuntimeAsset);
        }

        private static async Task<AsyncOperationHandle<BootSettings>> InitialiseBootSettingsAsset(string key)
        {
            var handle = Addressables.LoadAssetAsync<BootSettings>(key);
            await handle.Task;

            switch (handle.Status)
            {
                case AsyncOperationStatus.Succeeded:
                    await handle.Result.Initialise();
                    break;
                case AsyncOperationStatus.Failed:
                    Debug.LogError(handle.OperationException);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            return handle;
        }

        private static AsyncOperationHandle<BootSettings> InitialiseBootSettingsAssetSync(string key)
        {
            var handle = Addressables.LoadAssetAsync<BootSettings>(key);
            var result = handle.WaitForCompletion();
            result.InitializeSync();
            return handle;
        }

        private static void Cleanup(AsyncOperationHandle<BootSettings> handle)
        {
            if(!handle.IsValid()) return;
            handle.Result.Cleanup();
            Addressables.Release(handle);
        }
    }
}