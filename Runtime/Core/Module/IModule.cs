namespace IIMLib.Core.Module
{
    public interface IModule
    {
        public IModuleHolder Owner { get; set; }
        public virtual void OnRemove(){}
    }
}