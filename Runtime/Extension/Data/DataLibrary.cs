using System;
using System.Collections.Generic;

namespace IIMLib.Extension.Data
{
    public class DataLibrary
    {
        private readonly Dictionary<Type, IDataEntry> _DataEntries = new();
        
        public T Get<T>() where T : DataEntry<T> => (T)_DataEntries[typeof(T)].CloneUntyped();
        public T GetSave<T>() where T : DataEntry<T> =>  _DataEntries.TryGetValue(typeof(T), out var entry) ? (T)entry.CloneUntyped() : null;
        public T GetOrigin<T>() where T : DataEntry<T> => (T)_DataEntries[typeof(T)];
        public T GetOrDefault<T>() where T : DataEntry<T>, new() => _DataEntries.TryGetValue(typeof(T), out var entry) ? entry.CloneUntyped() as T : new T();
        public void ClearAll() =>  _DataEntries.Clear();
        public void Clear<T>() => _DataEntries.Remove(typeof(T));
        public T GetCleanData<T>() where T : DataEntry<T>, new() => _DataEntries.TryGetValue(typeof(T), out var entry) ? ((T)entry).CreateCleanDataEntry() : new T().CreateCleanDataEntry();

        public T Update<T>(T data) where T : DataEntry<T>, new()
        {
            if (data == null) throw new ArgumentNullException(nameof(data));
            
            if (_DataEntries.TryGetValue(typeof(T), out var entry))
            {
                ((T)entry).Merge(data);
            }
            else
            {
                var instance = new T();
                instance.Merge(data);
                _DataEntries[typeof(T)] = instance;
            }
            
            return Get<T>();
        }

        public void Merge(DataLibrary library)
        {
            foreach (var (key, data) in library._DataEntries)
            {
                if(!_DataEntries.TryGetValue(key, out var entry)) continue;
                entry.MergeFrom(data);
            }
        }

        public DataLibrary Clone()
        {
            var library = new DataLibrary();

            foreach (var (key, data) in _DataEntries)
            {
                library._DataEntries[key] = data.CloneUntyped();
            }

            return library;
        }
    }
}