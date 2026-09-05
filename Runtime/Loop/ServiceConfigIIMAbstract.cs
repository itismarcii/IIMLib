using System;
using System.Collections.Generic;
using IIMLib.Core;
using IIMLib.Core.Logger;
using IIMLib.Core.Message;
using UnityEngine;

namespace IIMLib.Loop
{
    public abstract class ServiceConfigIIMAbstract : ScriptableObject, IServiceConfig
    {
        public virtual IEnumerable<(Type, IService)> ServiceList
        {
            get
            {
                yield return (typeof(IGameLoopService<GameManagerIIM>), new GameLoopServiceIIM<GameManagerIIM>());
                yield return (typeof(IMessageService), new MessageService());
                yield return (typeof(ILoggerService), new LoggerService());
            }
        }
    }
}
