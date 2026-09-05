namespace IIMLib.Extension.Data
{
    public interface IDataEntry
    {
        IDataEntry Clone();
        void Override(IDataEntry source);
    }
}
