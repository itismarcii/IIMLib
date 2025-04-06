using System;
using System.Threading.Tasks;
using UnityEngine;

namespace IIMLib.Boot.Runtime
{
    public class BootSettings : ScriptableObject
    {
        private const string CLONE_SUFFIX = "(Clone)";
        
        [SerializeField] public GameObject[] Standalone = Array.Empty<GameObject>();
        [SerializeField, Space(10)] public GameObject[] Container = Array.Empty<GameObject>();
        
        private GameObject[] _Instances { get; set; }
        private GameObject _RuntimeContainer { get; set; }

        public async Task Initialise()
        {
            if (Container.Length > 0)
            {
                _RuntimeContainer = new GameObject($"{name}_Container");
                DontDestroyOnLoad(_RuntimeContainer);
            }
            
            _Instances = new GameObject[Standalone.Length + Container.Length];

            for (var i = 0; i < Standalone.Length; i++)
            {
                if(!Standalone[i]) 
                {
#if UNITY_EDITOR
                    Debug.LogWarning($"[{name}] Entity at index {i} is null.");
#endif 
                    continue;
                }

                var instance = InstantiateAsync(Standalone[i]);
                
                while (!instance.isDone) await Task.Yield();
                
                instance.Result[0].name = Standalone[i].name;
                _Instances[i] = instance.Result[0];
            }
            
            for (var i = 0; i < Container.Length; i++)
            {
                if(!Container[i]) 
                {
#if UNITY_EDITOR
                    Debug.LogWarning($"[{name}] Container Entity at index {i} is null.");
#endif
                    continue;
                }

                var instance = InstantiateAsync(Container[i], _RuntimeContainer.transform);
                
                while (!instance.isDone) await Task.Yield();
                
                instance.Result[0].name = Standalone[i].name;
                _Instances[Standalone.Length + i] = instance.Result[0];
            }
        }

        public void InitializeSync()
        {
            if (Container.Length > 0)
            {
                _RuntimeContainer = new GameObject($"{name}_Container");
                if (Application.isPlaying) DontDestroyOnLoad(_RuntimeContainer);
            }            

            _Instances = new GameObject[Standalone.Length + Container.Length];
            
            for (var i = 0; i < Standalone.Length; i++)
            {
                if (!Standalone[i]) 
                {
#if UNITY_EDITOR
                    Debug.LogWarning($"[{name}] Entity at index {i} is null.");
#endif                    
                    continue;
                }
                
                var instance = Instantiate(Standalone[i]);
                instance.name = Standalone[i].name;
                _Instances[i] = instance;

                if(instance.TryGetComponent(out IBoot boot)) boot.Initialize();
                Debug.Log($"{Standalone[i].name} initialized");
            }
            
            for (var i = 0; i < Container.Length; i++)
            {
                if (!Container[i])
                {
#if UNITY_EDITOR
                    Debug.LogWarning($"[{name}] Container Entity at index {i} is null.");
#endif
                    continue;
                }
                
                var instance = Instantiate(Container[i], _RuntimeContainer.transform);
                instance.name = Standalone[i].name;
                _Instances[Standalone.Length + i] = instance;

                if(instance.TryGetComponent(out IBoot boot)) boot.Initialize();
                Debug.Log($"{Standalone[i].name} initialized");
            }
        }

        public void Cleanup()
        {
            foreach (var instance in _Instances)
            {
                if(!instance) continue;
                
                if(Application.isPlaying) Destroy(instance);
                else DestroyImmediate(instance);
            }
            
            _Instances = null;
            
            if(Application.isPlaying) Destroy(_RuntimeContainer);
            else DestroyImmediate(_RuntimeContainer);
        }
    }
}