using System;
using IIMLib.Core;

namespace IIMLib.Loop
{
    public interface IGameLoopService<T> : IService
    {
        void Subscribe(object subscriber, Action<float> action, GameLoopType gameLoopType);
        void UnSubscribe(object subscriber, GameLoopType gameLoopType);
        void UnSubscribe(object subscriber);
        void Pause(object subscriber);
        void Resume(object subscriber);
        void Update(T updater, float deltaTime);
        void FixedUpdate(T updater, float fixedDeltaTime);
        void LateUpdate(T updater, float deltaTime);
    }
}
