using System;
using IIMLib.Boot.Runtime;
using IIMLib.Core;
using IIMLib.Editor;
using UnityEngine;

namespace IIMLib.Loop
{
    public abstract class GameManagerIIMAbstract : MonoBehaviour, IBoot
    {
        [field: SerializeField, RequireInterface(typeof(IServiceConfig))] private UnityEngine.Object _ServiceConfig { get; set; }
        protected IServiceConfig ServiceConfig => _ServiceConfig as IServiceConfig;
        
        public static GameManagerIIMAbstract Instance { get; protected set; }
        public bool Initialized { get; protected set; }

        private void Awake()
        {
            if (IsInvalidDestroyedSingleton()) return;
            OnAwake();
        }

        public void Initialize()
        {
            if (IsInvalidDestroyedSingleton()) return;

            Instance = this;
            DontDestroyOnLoad(gameObject); 
            Init();
        }

        private bool IsInvalidDestroyedSingleton()
        {
            if (Instance == null || Instance == this) return false;
            Debug.LogWarning($"{gameObject.name} is already instantiated. {gameObject} destroyed.");
            Destroy(gameObject);
            return true;
        }

        protected virtual void Init()
        {
            try
            {
                ServiceLocator.Initialize(ServiceConfig);
                Initialized = true;
            }
            catch (Exception e)
            {
                enabled = Initialized = false;
                Console.WriteLine(e);
                throw;
            }
        }
        
        protected virtual void OnAwake() {}
    }
}