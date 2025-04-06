using System;
using System.Collections.Generic;
using IIMLib.Core;
using UnityEngine;

namespace IIMLib.Loop
{
    [CreateAssetMenu(menuName = "IIM/Service/Base Config", fileName = "Base Service Config")]
    public class ServiceConfigIIMAbstract : ScriptableObject, IServiceConfig
    {
        public IEnumerable<(Type, IService)> ServiceList => GetServiceConfig();

        protected virtual List<(Type, IService)> GetServiceConfig()
        {
            return new List<(Type, IService)>()
            {
                (typeof(IGameLoopService<GameLoopType>), new GameLoopServiceIIM<GameManagerIIM>()),
                (typeof(IMessageService), new MessageService()),
                (typeof(ILoggerService), new LoggerService())
            };
        }
    }
}