using System;
using System.Collections.Generic;

namespace IIMLib.Core
{
    public interface IServiceConfig
    {
        /// <summary>
        /// A collection of all services in order of how they should be initialized. <br/>
        /// Type is the key that will access the service.<br/>
        /// IService is the service object that will be used inside the ServiceLocator.
        /// </summary>
        public IEnumerable<(Type, IService)> ServiceList { get; }
    }
}