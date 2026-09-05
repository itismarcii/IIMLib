using IIMLib.Core;
using IIMLib.Core.Logger;
using IIMLib.Core.Message;
using UnityEngine;

namespace IIMLib.Loop
{
    [RequireComponent(typeof(LoadingScreenHandler))]
    public class GameManagerIIM : GameManagerIIMAbstract
    {
        [SerializeField] private LogLevel _logLevel = LogLevel.Info;
        [SerializeField] private LoadingScreenHandler _loadingScreenHandler;

        public static IGameLoopService<GameManagerIIM> LoopService { get; private set; }

        public bool Initialized { get; private set; }
        public float DeltaTime { get; private set; }
        public float FixedDeltaTime { get; private set; }

        public LoadingScreenHandler LoadingScreenHandler => _loadingScreenHandler;

        protected override void InitializeServices()
        {
            if (ServiceConfig == null)
            {
                Debug.LogError($"{nameof(GameManagerIIM)} requires a ServiceConfig implementing {nameof(IServiceConfig)}.");
                enabled = false;
                return;
            }

            ServiceLocator.Initialize(ServiceConfig);

            if (ServiceLocator.TryGet<ILoggerService>(out var logger))
                logger.SetLogLevel(_logLevel);

            ServiceLocator.TryGet(out IGameLoopService<GameManagerIIM> loopService);
            LoopService = loopService;

            Initialized = LoopService != null;
            enabled = Initialized;

            if (ServiceLocator.TryGet<IMessageService>(out var messageService))
                messageService.Publish(new ServicesInitializedMessage());
        }

        private void Update()
        {
            if (!Initialized)
                return;

            DeltaTime = Time.deltaTime;
            LoopService.Update(this, DeltaTime);
        }

        private void FixedUpdate()
        {
            if (!Initialized)
                return;

            FixedDeltaTime = Time.fixedDeltaTime;
            LoopService.FixedUpdate(this, FixedDeltaTime);
        }

        private void LateUpdate()
        {
            if (!Initialized)
                return;

            LoopService.LateUpdate(this, Time.deltaTime);
        }
    }
}
