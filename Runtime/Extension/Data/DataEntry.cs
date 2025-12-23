using System;

namespace IIMLib.Extension.Data
{
    public abstract class DataEntry<T> : IDataEntry where T : DataEntry<T>
    {
        public abstract void Merge(T data);
        public abstract DataEntry<T> CreateCleanDataEntry();
        public abstract DataEntry<T> Clone();

        void IDataEntry.MergeFrom(IDataEntry entry)
        {
            if(entry is not T typeData) throw new InvalidOperationException($"Cannot merge {entry.GetType().Name} into {typeof(T).Name}");
            
            Merge(typeData);
        }

        IDataEntry IDataEntry.CloneUntyped() => Clone();
    }
}
