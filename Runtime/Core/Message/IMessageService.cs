using System;

namespace IIMLib.Core
{
    public interface IMessageService : IService
    {
        public void Subscribe<T>(in Action<T> message) where T : struct, IMessage;
        public void Unsubscribe<T>(in Action<T> message) where T : struct, IMessage;
        public void Publish<T>(T message) where T : struct, IMessage;
    }
}