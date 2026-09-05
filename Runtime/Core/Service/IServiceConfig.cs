using System;
using System.Collections.Generic;

namespace IIMLib.Core
{
    public interface IServiceConfig
    {
        IEnumerable<(Type, IService)> ServiceList { get; }
    }
}
