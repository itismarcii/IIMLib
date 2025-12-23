using System;

namespace IIMLib.Extension.Data.Example
{
    /// <summary>
    /// Example data entry demonstrating a patch-style data model.
    /// </summary>
    /// <remarks>
    /// This data entry separates update intent from resolved state.
    /// Nullable properties represent partial update intent, while non-nullable
    /// properties expose a safe, resolved view for runtime usage.
    /// </remarks>
    public sealed class ExampleDataEntry : DataEntry<ExampleDataEntry>
    {
        /// <summary>
        /// Gets the optional value representing update intent.
        /// </summary>
        /// <remarks>
        /// This property is nullable to distinguish between "no change" and an explicit
        /// assignment. A <c>null</c> value indicates that this entry should not modify
        /// existing data during a merge operation.
        /// </remarks>
        public int? Value { get; set; }

        /// <summary>
        /// Gets the resolved, non-nullable value.
        /// </summary>
        /// <remarks>
        /// This property never returns <c>null</c>. If no explicit value was provided,
        /// a default of <c>0</c> is returned. This accessor is intended for runtime and
        /// gameplay logic where nullable intent is not meaningful.
        /// </remarks>
        public int ValueClean => Value ?? 0;
        
        /// <summary>
        /// Merges the provided data entry into this instance.
        /// </summary>
        /// <remarks>
        /// Only non-null intent values are applied. This allows instances of
        /// <see cref="ExampleDataEntry"/> to function as patch objects rather than
        /// complete state replacements.
        /// </remarks>
        /// <param name="data">The data entry containing changes to apply.</param>
        /// <exception cref="ArgumentNullException">
        /// Thrown if <paramref name="data"/> is <c>null</c>.
        /// </exception>
        public override void Merge(ExampleDataEntry data)
        {
            if (data == null) throw new ArgumentNullException(nameof(data));

            Value = data.Value ?? Value;
        }

        /// <summary>
        /// Creates a resolved, non-nullable representation of this data entry.
        /// </summary>
        /// <remarks>
        /// This method produces a clean snapshot suitable for runtime use by resolving
        /// nullable intent into concrete values.
        /// </remarks>
        /// <returns>A clean data entry instance.</returns>
        public override ExampleDataEntry CreateCleanDataEntry()
        {
            return new ExampleDataEntry
            {
                Value = ValueClean
            };
        }

        /// <summary>
        /// Creates an exact copy of this data entry.
        /// </summary>
        /// <remarks>
        /// The cloned instance preserves nullable intent information to ensure that
        /// merge semantics remain consistent across snapshots.
        /// </remarks>
        /// <returns>A cloned data entry instance.</returns>
        public override ExampleDataEntry Clone()
        {
            return new ExampleDataEntry
            {
                Value = Value
            };
        }
    }
}