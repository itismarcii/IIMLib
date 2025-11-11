namespace IIMLib.Core.Module
{
    public abstract class BaseModule : IModule
    {
        public IModuleHolder Owner { get; set; }
        public virtual void OnRemove() { }
    }
}