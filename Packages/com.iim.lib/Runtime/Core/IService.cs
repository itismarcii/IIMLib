namespace IIMLib.Core
{
    public interface IService
    {
        public virtual string IdentifierName => null;
        
        void Initialize();
    }
}
