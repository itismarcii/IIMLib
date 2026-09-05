using System;
using System.Threading.Tasks;
using UnityEngine;

namespace IIMLib.Boot.Runtime
{
    [CreateAssetMenu(menuName = "IIM/Boot/Boot Settings", fileName = "BootSettings")]
    public sealed class BootSettings : ScriptableObject
    {
        [SerializeField] public GameObject[] Standalone = Array.Empty<GameObject>();
        [SerializeField] public GameObject[] Container = Array.Empty<GameObject>();

        private GameObject[] _instances;
        private GameObject _runtimeContainer;

        public async Task Initialise()
        {
            EnsureRuntimeContainer();

            var standaloneCount = Standalone?.Length ?? 0;
            var containerCount = Container?.Length ?? 0;
            _instances = new GameObject[standaloneCount + containerCount];

            var index = 0;

            for (var i = 0; i < standaloneCount; i++)
            {
                var prefab = Standalone[i];
                if (prefab == null)
                {
                    Debug.LogWarning($"{name}: Standalone boot prefab at index {i} is null.");
                    index++;
                    continue;
                }

                // Unity 2022.3 has Object.Instantiate, not Object.InstantiateAsync.
                // Instantiation stays on Unity's main thread; yielding between entries
                // keeps this API asynchronous without touching the thread pool.
                var instance = Instantiate(prefab);
                instance.name = prefab.name;
                _instances[index++] = instance;
                InitializeBootComponent(instance);

                await Task.Yield();
            }

            for (var i = 0; i < containerCount; i++)
            {
                var prefab = Container[i];
                if (prefab == null)
                {
                    Debug.LogWarning($"{name}: Container boot prefab at index {i} is null.");
                    index++;
                    continue;
                }

                var instance = Instantiate(prefab, _runtimeContainer.transform);
                instance.name = prefab.name;
                _instances[index++] = instance;
                InitializeBootComponent(instance);

                await Task.Yield();
            }
        }

        public void InitializeSync()
        {
            EnsureRuntimeContainer();

            var standaloneCount = Standalone?.Length ?? 0;
            var containerCount = Container?.Length ?? 0;
            _instances = new GameObject[standaloneCount + containerCount];

            var index = 0;

            for (var i = 0; i < standaloneCount; i++)
            {
                var prefab = Standalone[i];
                if (prefab == null)
                {
                    Debug.LogWarning($"{name}: Standalone boot prefab at index {i} is null.");
                    index++;
                    continue;
                }

                var instance = Instantiate(prefab);
                instance.name = prefab.name;
                _instances[index++] = instance;
                InitializeBootComponent(instance);
            }

            for (var i = 0; i < containerCount; i++)
            {
                var prefab = Container[i];
                if (prefab == null)
                {
                    Debug.LogWarning($"{name}: Container boot prefab at index {i} is null.");
                    index++;
                    continue;
                }

                var instance = Instantiate(prefab, _runtimeContainer.transform);
                instance.name = prefab.name;
                _instances[index++] = instance;
                InitializeBootComponent(instance);
            }
        }

        public void Cleanup()
        {
            if (_instances != null)
            {
                for (var i = 0; i < _instances.Length; i++)
                {
                    if (_instances[i] != null)
                        Destroy(_instances[i]);
                }

                _instances = null;
            }

            if (_runtimeContainer != null)
            {
                Destroy(_runtimeContainer);
                _runtimeContainer = null;
            }
        }

        private void EnsureRuntimeContainer()
        {
            if (_runtimeContainer != null || Container == null || Container.Length == 0)
                return;

            _runtimeContainer = new GameObject($"{name}_Container");

            if (Application.isPlaying)
                DontDestroyOnLoad(_runtimeContainer);
        }

        private static void InitializeBootComponent(GameObject instance)
        {
            if (instance.TryGetComponent<IBoot>(out var boot))
                boot.Initialize();
        }
    }
}
