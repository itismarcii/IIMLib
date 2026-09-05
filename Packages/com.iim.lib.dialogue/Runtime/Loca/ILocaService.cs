using IIMLib.Core;

namespace Loca
{
    public interface ILocaService : IService
    {
        public string Get(string key, params (string, string)[][] args);
    }
}