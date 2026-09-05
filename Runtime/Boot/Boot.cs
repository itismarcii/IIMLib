using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace IIMLib.Boot.Runtime
{
    public static class Boot
    {
        private const string RuntimeAsset = "BootSettings_Runtime";
        private const string EditorAsset = "BootSettings_Editor";

        private static AsyncOperationHandle<BootSettings> _runtimeBootSettingsHandle;
        private static AsyncOperationHandle<BootSettings> _editorBootSettingsHandle;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSplashScreen)]
        private static void Initialize()
        {
            Application.quitting -= Cleanup;
            Application.quitting += Cleanup;

#if UNITY_EDITOR
            _runtimeBootSettingsHandle = InitializeBootSettingsAssetSync(RuntimeAsset);
            _editorBootSettingsHandle = InitializeBootSettingsAssetSync(EditorAsset);
#else
            _ = InitializePlayerAsync();
#endif
        }

        private static async Task InitializePlayerAsync()
        {
            _runtimeBootSettingsHandle = await InitialiseBootSettingsAsset(RuntimeAsset);
        }

        private static async Task<AsyncOperationHandle<BootSettings>> InitialiseBootSettingsAsset(string address)
        {
            var handle = Addressables.LoadAssetAsync<BootSettings>(address);
            await handle.Task;

            if (handle.Status != AsyncOperationStatus.Succeeded || handle.Result == null)
            {
                Debug.LogError($"Failed to load boot settings at address '{address}'.");
                return handle;
            }

            await handle.Result.Initialise();
            return handle;
        }

        private static AsyncOperationHandle<BootSettings> InitializeBootSettingsAssetSync(string address)
        {
            var handle = Addressables.LoadAssetAsync<BootSettings>(address);
            var result = handle.WaitForCompletion();

            if (handle.Status != AsyncOperationStatus.Succeeded || result == null)
            {
                Debug.LogWarning($"Boot settings at address '{address}' could not be loaded.");
                return handle;
            }

            result.InitializeSync();
            return handle;
        }

        private static void Cleanup()
        {
            CleanupHandle(ref _runtimeBootSettingsHandle);
            CleanupHandle(ref _editorBootSettingsHandle);
        }

        private static void CleanupHandle(ref AsyncOperationHandle<BootSettings> handle)
        {
            if (!handle.IsValid())
                return;

            if (handle.Result != null)
                handle.Result.Cleanup();

            Addressables.Release(handle);
            handle = default;
        }
    }
}
