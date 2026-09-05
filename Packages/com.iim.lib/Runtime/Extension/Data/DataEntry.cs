namespace IIMLib.Extension.Data
{
    public abstract class DataEntry<T> : IDataEntry where T : DataEntry<T>, new()
    {
        public virtual IDataEntry Clone()
        {
            var result = new T();
            result.Override(this);
            return result;
        }

        public abstract void Override(IDataEntry source);
    }
}
