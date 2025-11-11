namespace Extension.Data
{
    public abstract class DataEntry
    {
        public abstract void Merge<T>(T data) where T : DataEntry;
        public abstract DataEntry CreateCleanDataEntry();
        public abstract DataEntry Clone();
    }
}
