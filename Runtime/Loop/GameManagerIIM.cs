using System;
using IIMLib.Core;
using UnityEngine;

namespace IIMLib.Loop
{
    [RequireComponent(typeof(LoadingScreenHandler))]
    public class GameManagerIIM : GameManagerIIMAbstract
    {
        [field: SerializeField] public LogLevel LogLevel { get; protected set; } = LogLevel.Info;
        [field: SerializeField] protected LoadingScreenHandler LoadingScreenHandler { get; private set;}

        protected static IGameLoopService<GameLoopType> LoopService;
        public virtual bool IsPaused { get; protected set; }
        public float FixedDeltaTime { get; private set; }
        public float DeltaTime { get; private set; }
        
        protected override void Init()
        {
            try
            {
                ServiceLocator.Initialize(ServiceConfig);
                ServiceLocator.Get<ILoggerService>().SetLogLevel(LogLevel);
                LoopService = ServiceLocator.Get<IGameLoopService<GameLoopType>>();
                Initialized = enabled = LoopService != null;
                ServiceLocator.Get<IMessageService>().Publish(new ServicesInitializedMessage());
            }
            catch (Exception e)
            {
                enabled = Initialized = false;
                Console.WriteLine(e);
                throw;
            }
        }
        
        public void OnValidate()
        {
            LoadingScreenHandler ??= GetComponent<LoadingScreenHandler>();
            AfterValidate();
        }

        private void Update()
        {
            if(IsPaused) return;
            
            DeltaTime = Time.deltaTime;
            
            BeforeUpdate();
            LoopService.Update(this, DeltaTime);
            AfterUpdate();
        }

        private void FixedUpdate()
        {
            if(IsPaused) return;

            FixedDeltaTime = Time.fixedTime;
            
            BeforeFixedUpdate();
            LoopService.FixedUpdate(this, FixedDeltaTime);
            AfterFixedUpdate();
        }
        
        private void LateUpdate()
        {
            if(IsPaused) return;
            
            BeforeLateUpdate();
            LoopService.LateUpdate(this, DeltaTime);
            AfterLateUpdate();
        }

        protected virtual void AfterValidate() { }
        protected virtual void BeforeUpdate() { }
        protected virtual void AfterUpdate() { }
        protected virtual void BeforeFixedUpdate() { }
        protected virtual void AfterFixedUpdate() { }
        protected virtual void BeforeLateUpdate() { }
        protected virtual void AfterLateUpdate() { }
    }
}