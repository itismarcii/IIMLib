using System;
using System.Collections;
using System.Collections.Generic;

namespace IIMLib.Extension.Data
{
    /**
     * Typed collection of IDataEntry values keyed by their concrete runtime type.
     * The collection preserves insertion order.
     */
    public sealed class DataLibrary : IEnumerable<IDataEntry>
    {
        private readonly Dictionary<Type, IDataEntry> _entries;
        private readonly List<Type> _order;

        public int Count => _order.Count;

        public DataLibrary()
        {
            _entries = new Dictionary<Type, IDataEntry>();
            _order = new List<Type>();
        }

        public DataLibrary(IEnumerable<IDataEntry> entries) : this()
        {
            if (entries == null)
                return;

            foreach (var entry in entries)
                Set(entry);
        }

        public bool Contains<T>() where T : class, IDataEntry
            => _entries.ContainsKey(typeof(T));

        public bool Contains(Type type)
        {
            if (type == null)
                throw new ArgumentNullException(nameof(type));

            return _entries.ContainsKey(type);
        }

        public T Get<T>() where T : class, IDataEntry
        {
            if (TryGet<T>(out var value))
                return value;

            throw new KeyNotFoundException($"Data entry '{typeof(T).FullName}' does not exist.");
        }

        public IDataEntry Get(Type type)
        {
            if (type == null)
                throw new ArgumentNullException(nameof(type));

            if (_entries.TryGetValue(type, out var value))
                return value;

            throw new KeyNotFoundException($"Data entry '{type.FullName}' does not exist.");
        }

        public bool TryGet<T>(out T value) where T : class, IDataEntry
        {
            if (_entries.TryGetValue(typeof(T), out var entry))
            {
                value = entry as T;
                return value != null;
            }

            value = null;
            return false;
        }

        public bool TryGet(Type type, out IDataEntry value)
        {
            if (type == null)
            {
                value = null;
                return false;
            }

            return _entries.TryGetValue(type, out value);
        }

        public void Set<T>(T entry) where T : class, IDataEntry
            => Set((IDataEntry)entry);

        public void Set(IDataEntry entry)
        {
            if (entry == null)
                throw new ArgumentNullException(nameof(entry));

            var type = entry.GetType();

            if (!_entries.ContainsKey(type))
                _order.Add(type);

            _entries[type] = entry;
        }

        public bool Remove<T>() where T : class, IDataEntry
            => Remove(typeof(T));

        public bool Remove(Type type)
        {
            if (type == null)
                throw new ArgumentNullException(nameof(type));

            if (!_entries.Remove(type))
                return false;

            _order.Remove(type);
            return true;
        }

        public void Clear()
        {
            _entries.Clear();
            _order.Clear();
        }

        public DataLibrary Clone()
        {
            var result = new DataLibrary();

            for (var i = 0; i < _order.Count; i++)
            {
                var type = _order[i];
                var entry = _entries[type];
                result.Set(entry.Clone());
            }

            return result;
        }

        /**
         * Applies entries from source to this library.
         * Existing entries are overridden; missing entries are cloned and added.
         */
        public void Override(DataLibrary source)
        {
            if (source == null)
                throw new ArgumentNullException(nameof(source));

            foreach (var sourceEntry in source)
            {
                var type = sourceEntry.GetType();

                if (_entries.TryGetValue(type, out var targetEntry))
                    targetEntry.Override(sourceEntry);
                else
                    Set(sourceEntry.Clone());
            }
        }

        /**
         * Alias for Override; useful when treating one library as a patch.
         */
        public void Update(DataLibrary source) => Override(source);

        /**
         * Returns a clone of this library with source applied on top.
         */
        public DataLibrary Merge(DataLibrary source)
        {
            var result = Clone();
            result.Override(source);
            return result;
        }

        /**
         * Combines libraries from left to right. Later libraries override earlier libraries.
         */
        public static DataLibrary Combine(params DataLibrary[] libraries)
        {
            var result = new DataLibrary();

            if (libraries == null)
                return result;

            for (var i = 0; i < libraries.Length; i++)
            {
                var library = libraries[i];
                if (library != null)
                    result.Override(library);
            }

            return result;
        }

        public static DataLibrary Combine(IEnumerable<DataLibrary> libraries)
        {
            var result = new DataLibrary();

            if (libraries == null)
                return result;

            foreach (var library in libraries)
            {
                if (library != null)
                    result.Override(library);
            }

            return result;
        }

        public IEnumerator<IDataEntry> GetEnumerator()
        {
            for (var i = 0; i < _order.Count; i++)
                yield return _entries[_order[i]];
        }

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}
