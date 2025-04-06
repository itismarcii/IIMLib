using System;
using IIMLib.Core;

namespace IIMLib.Loop
{
    public interface IGameLoopService<in T> : IService where T : Enum
    {
        public void Subscribe(object subscriber, Action<float> action, T type);
        public void UnSubscribe(object subscriber, T type);
        public void UnSubscribe(object subscriber);
        public void PauseUpdate(object subscriber);
        public void PauseUpdate(object subscriber, T type);
        public void ResumeUpdate(object subscriber);
        public void ResumeUpdate(object subscriber,T type);
        public void Update(object updater, float deltaTime);
        public void FixedUpdate(object updater, float deltaTime);
        public void LateUpdate(object updater, float deltaTime);
    }
}