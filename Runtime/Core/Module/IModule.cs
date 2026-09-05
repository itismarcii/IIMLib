namespace IIMLib.Core.Module
{
    public interface IModule
    {
        IModuleHolder Owner { get; set; }

        void OnRemove()
        {
        }
    }
}
