using IIMLib.Boot.Runtime;
using IIMLib.Core;
using UnityEngine;

namespace IIMLib.Loop
{
    public abstract class GameManagerIIMAbstract : MonoBehaviour, IBoot
    {
        public static GameManagerIIMAbstract Instance { get; private set; }

        [field: SerializeField, RequireInterface(typeof(IServiceConfig))]
        protected UnityEngine.Object ServiceConfigObject { get; private set; }

        protected IServiceConfig ServiceConfig => ServiceConfigObject as IServiceConfig;

        public virtual void Initialize()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject);

            InitializeServices();
        }

        protected abstract void InitializeServices();

        protected virtual void OnDestroy()
        {
            if (Instance == this)
                Instance = null;
        }
    }
}
