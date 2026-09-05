using System;

namespace IIMLib.Core
{
    public interface IServiceConfigComponent
    {
        public (Type key, IService service) Config { get; }
    }
}