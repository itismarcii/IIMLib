using System;
using System.Collections.Generic;

namespace IIMLib.Extension.Data
{
    /// <summary>
    /// Centralized container for typed <see cref="DataEntry{T}"/> instances.
    /// 
    /// The library enforces controlled mutation via <see cref="Update{T}(T)"/> and
    /// exposes read-only snapshots through cloning to prevent accidental state leaks.
    /// </summary>
    public class DataLibrary
    {
        private readonly Dictionary<Type, IDataEntry> _DataEntries = new();
        
        /// <summary>
        /// Retrieves a cloned snapshot of the stored data entry of type <typeparamref name="T"/>.
        /// </summary>
        /// <typeparam name="T">The concrete data entry type.</typeparam>
        /// <returns>
        /// A cloned snapshot of the stored data entry.
        /// </returns>
        /// <exception cref="KeyNotFoundException">
        /// Thrown if no data entry of type <typeparamref name="T"/> exists in the library.
        /// </exception>
        public T Get<T>() where T : DataEntry<T> => (T)_DataEntries[typeof(T)].CloneUntyped();
        
        /// <summary>
        /// Retrieves a cloned snapshot of the stored data entry of type <typeparamref name="T"/>,
        /// or <c>null</c> if no such entry exists.
        /// </summary>
        /// <typeparam name="T">The concrete data entry type.</typeparam>
        /// <returns>
        /// A cloned snapshot of the stored data entry, or <c>null</c> if not present.
        /// </returns>
        public T GetSave<T>() where T : DataEntry<T> =>  _DataEntries.TryGetValue(typeof(T), out var entry) ? (T)entry.CloneUntyped() : null;
        
        /// <summary>
        /// Retrieves the internal mutable instance of the data entry of type <typeparamref name="T"/>.
        /// </summary>
        /// <remarks>
        /// This method exposes internal state and should be used sparingly.
        /// External code should prefer <see cref="Get{T}"/>, <see cref="GetCleanData{T}"/> or <see cref="GetOrDefault{T}"/>.
        /// </remarks>
        /// <typeparam name="T">The concrete data entry type.</typeparam>
        /// <returns>The stored mutable data entry instance.</returns>
        /// <exception cref="KeyNotFoundException">
        /// Thrown if no data entry of type <typeparamref name="T"/> exists in the library.
        /// </exception>
        public T GetOrigin<T>() where T : DataEntry<T> => (T)_DataEntries[typeof(T)];
        
        /// <summary>
        /// Retrieves a cloned snapshot of the stored data entry of type <typeparamref name="T"/>,
        /// or returns a new default instance if no entry exists.
        /// </summary>
        /// <typeparam name="T">The concrete data entry type.</typeparam>
        /// <returns>
        /// A cloned snapshot of the stored data entry, or a new default instance.
        /// </returns>
        public T GetOrDefault<T>() where T : DataEntry<T>, new() => _DataEntries.TryGetValue(typeof(T), out var entry) ? entry.CloneUntyped() as T : new T();
        
        /// <summary>
        /// Retrieves a resolved, non-nullable representation of the data entry of type
        /// <typeparamref name="T"/>.
        /// </summary>
        /// <remarks>
        /// If no entry exists, a new default instance is created and resolved.
        /// The returned instance is always a snapshot and never exposes internal state.
        /// </remarks>
        /// <typeparam name="T">The concrete data entry type.</typeparam>
        /// <returns>A clean, resolved data entry instance.</returns>
        public T GetCleanData<T>() where T : DataEntry<T>, new() => _DataEntries.TryGetValue(typeof(T), out var entry) ? ((T)entry).CreateCleanDataEntry() : new T().CreateCleanDataEntry();

        /// <summary>
        /// Removes all data entries from the library.
        /// </summary>
        public void ClearAll() =>  _DataEntries.Clear();
        
        /// <summary>
        /// Removes the data entry of type <typeparamref name="T"/> from the library.
        /// </summary>
        /// <typeparam name="T">The concrete data entry type.</typeparam>
        public void Clear<T>() => _DataEntries.Remove(typeof(T));
        
        /// <summary>
        /// Applies a partial update (data) to the stored data entry of type <typeparamref name="T"/>.
        /// </summary>
        /// <remarks>
        /// If no entry exists, a new instance is created and initialized using the provided data.
        /// The stored data is mutated internally, while the returned value is a cloned snapshot.
        /// </remarks>
        /// <typeparam name="T">The concrete data entry type.</typeparam>
        /// <param name="data">The partial data used to update the stored entry.</param>
        /// <returns>A cloned snapshot of the updated data entry.</returns>
        /// <exception cref="ArgumentNullException">
        /// Thrown if <paramref name="data"/> is <c>null</c>.
        /// </exception>
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

        /// <summary>
        /// Replaces the stored data entry of type <typeparamref name="T"/> with the provided data.
        /// </summary>
        /// <remarks>
        /// This method performs a full replacement of the existing data entry rather than
        /// applying a partial update.
        /// <para/>
        /// Unlike <see cref="Update{T}(T)"/>, which merges non-null intent into the current
        /// state, <c>Override</c> discards any previously stored state and replaces it with
        /// a new instance initialized from <paramref name="data"/>.
        /// <para/>
        /// Fields that are not explicitly set in <paramref name="data"/> will remain unset
        /// in the resulting entry, allowing this method to be used to reset values back to
        /// their null state.</remarks>
        /// <typeparam name="T">The concrete data entry type.</typeparam>
        /// <param name="data">
        /// The data entry describing the new authoritative state.
        /// </param>
        /// <returns>
        /// A cloned snapshot of the overridden data entry.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// Thrown if <paramref name="data"/> is <c>null</c>.
        /// </exception>
        public T Override<T>(T data) where T : DataEntry<T>, new()
        {
            if (data == null) throw new ArgumentNullException(nameof(data));

            var instance = new T();
            instance.Merge(data);
            _DataEntries[typeof(T)] = instance;

            return Get<T>();
        }
        
        /// <summary>
        /// Merges all compatible data entries from another <see cref="DataLibrary"/> into this one.
        /// </summary>
        /// <remarks>
        /// Only data entries that already exist in this library are merged.
        /// Missing entries are ignored.
        /// </remarks>
        /// <param name="library">The source data library.</param>
        public void Merge(DataLibrary library)
        {
            foreach (var (key, data) in library._DataEntries)
            {
                if(!_DataEntries.TryGetValue(key, out var entry)) continue;
                entry.MergeFrom(data);
            }
        }

        /// <summary>
        /// Creates a deep copy of this <see cref="DataLibrary"/> and all contained data entries.
        /// </summary>
        /// <returns>
        /// A new <see cref="DataLibrary"/> instance containing cloned data entries.
        /// </returns>
        public DataLibrary Clone()
        {
            var library = new DataLibrary();

            foreach (var (key, data) in _DataEntries)
            {
                library._DataEntries[key] = data.CloneUntyped();
            }

            return library;
        }

        
        /// <summary>
        /// Creates a new <see cref="DataLibrary"/> by combining two source libraries.
        /// </summary>
        /// <remarks>
        /// Neither input library is modified.
        /// 
        /// When both libraries contain a data entry of the same type, the entry from
        /// <paramref name="two"/> is merged into the entry from <paramref name="one"/>
        /// using the data entry's merge rules.
        /// 
        /// Entries that exist in only one library are cloned directly into the result.
        /// </remarks>
        /// <param name="one">The base data library.</param>
        /// <param name="two">The data library to merge on top of the base.</param>
        /// <returns>
        /// A new <see cref="DataLibrary"/> containing the combined data.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// Thrown if either <paramref name="one"/> or <paramref name="two"/> is <c>null</c>.
        /// </exception>
        public static DataLibrary Combine(DataLibrary one, DataLibrary two)
        {
            if (one == null) throw new ArgumentNullException(nameof(one));
            if (two == null) throw new ArgumentNullException(nameof(two));

            var result = one.Clone();

            foreach (var (key, data) in two._DataEntries)
            {
                if (result._DataEntries.TryGetValue(key, out var entry))
                {
                    entry.MergeFrom(data);
                }
                else
                {
                    result._DataEntries[key] = data.CloneUntyped();
                }
            }

            return result;
        }

        /// <summary>
        /// Combines all data entries from <paramref name="other"/> into <paramref name="target"/>
        /// and aliases <paramref name="other"/> to reference <paramref name="target"/>.
        /// </summary>
        /// <remarks>
        /// This method performs a destructive fusion with move semantics:
        /// <list type="bullet">
        /// <item>
        /// All data entries contained in <paramref name="other"/> are merged into
        /// <paramref name="target"/>.
        /// </item>
        /// <item>
        /// When both libraries contain a data entry of the same type, the entry from
        /// <paramref name="other"/> is merged into the entry in <paramref name="target"/>
        /// using <see cref="IDataEntry.MergeFrom"/>.
        /// </item>
        /// <item>
        /// When a data entry exists only in <paramref name="other"/>, ownership of that
        /// entry is transferred directly to <paramref name="target"/> without cloning.
        /// </item>
        /// <item>
        /// After fusion, <paramref name="other"/> is reassigned to reference
        /// <paramref name="target"/>, causing both parameters to point to the same
        /// <see cref="DataLibrary"/> instance.
        /// </item>
        /// </list>
        /// 
        /// As a result, the original <see cref="DataLibrary"/> instance referenced by
        /// <paramref name="other"/> is discarded and should not be used further.
        /// Any external references to that instance will not observe future changes.
        /// </remarks>
        /// <param name="target">
        /// The destination data library that will contain the fused data.
        /// </param>
        /// <param name="other">
        /// The source data library whose data will be fused and whose reference will be
        /// redirected to <paramref name="target"/>.
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// Thrown if <paramref name="target"/> or <paramref name="other"/> is <c>null</c>.
        /// </exception>
        /// <exception cref="InvalidOperationException">
        /// Thrown if <paramref name="target"/> and <paramref name="other"/> refer to the
        /// same <see cref="DataLibrary"/> instance.
        /// </exception>
        public static void CombineAndAliasInto(ref DataLibrary target, ref DataLibrary other)
        {
            if (target == null) throw new ArgumentNullException(nameof(target));
            if (other == null) throw new ArgumentNullException(nameof(other));
            if (ReferenceEquals(target, other)) throw new InvalidOperationException("Cannot fuse a DataLibrary into itself.");

            foreach (var (key, data) in other._DataEntries)
            {
                if (target._DataEntries.TryGetValue(key, out var entry))
                {
                    entry.MergeFrom(data);
                    
                }
                else
                {
                    target._DataEntries[key] = data;
                }
            }

            other = target;
        }
    }
}