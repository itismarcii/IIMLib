namespace IIMLib.Extension.Data
{
    public interface IDataEntry
    {
        void MergeFrom(IDataEntry other);
        IDataEntry CloneUntyped();
    }
}