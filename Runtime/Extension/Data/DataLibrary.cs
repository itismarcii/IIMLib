using System;
using System.Collections.Generic;

namespace Extension.Data
{
    public class DataLibrary
    {
        private readonly Dictionary<Type, DataEntry> _DataEntries = new();
        
        public T Get<T>() where T : DataEntry => (T)_DataEntries[typeof(T)];
        public T GetOrDefault<T>() where T : DataEntry, new() => _DataEntries.TryGetValue(typeof(T), out var entry) ? entry as T : new T();
        public void ClearAll() =>  _DataEntries.Clear();
        public void Clear<T>() where T : DataEntry, new() => _DataEntries.Remove(typeof(T));
        public T GetCleanData<T>() where T : DataEntry, new() => GetOrDefault<T>().CreateCleanDataEntry() as T;

        public T Update<T>(T data) where T : DataEntry, new()
        {
            if (data == null) return null;

            if (_DataEntries.TryGetValue(typeof(T), out var entry))
            {
                entry.Merge(data);
                return Get<T>();
            }

            var instance = new T();
            instance.Merge(data);
            _DataEntries[typeof(T)] = instance;
            return instance;
        }

        public void Merge(DataLibrary library)
        {
            foreach (var (key, data) in library._DataEntries)
            {
                if(!_DataEntries.TryGetValue(key, out var entry)) continue;
                entry.Merge(data);
            }
        }

        public DataLibrary Clone()
        {
            var library = new DataLibrary();

            foreach (var (key, data) in _DataEntries)
            {
                library._DataEntries[key] = data.Clone();
            }

            return library;
        }
    }
}