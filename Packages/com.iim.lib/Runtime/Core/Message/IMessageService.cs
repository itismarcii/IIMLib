using System;

namespace IIMLib.Core.Message
{
    public interface IMessageService : IService
    {
        void Subscribe<T>(in Action<T> message) where T : struct, IMessage;
        void UnSubscribe<T>(in Action<T> message) where T : struct, IMessage;
        void Publish<T>(in T message) where T : struct, IMessage;
    }
}
