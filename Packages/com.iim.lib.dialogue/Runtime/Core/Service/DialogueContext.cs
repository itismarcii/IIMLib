using System;
using System.Collections.Generic;
using System.Globalization;

namespace IIMLib.Dialogue.Service
{
    public sealed class DialogueContext
    {
        private readonly Dictionary<string, ContextValue> _variables;

        public int Count => _variables.Count;

        public DialogueContext()
            : this(StringComparer.Ordinal)
        {
        }

        public DialogueContext(IEqualityComparer<string> comparer)
        {
            _variables = new Dictionary<string, ContextValue>(comparer ?? StringComparer.Ordinal);
        }

        public DialogueContext Set<T>(string key, T value)
        {
            if (string.IsNullOrWhiteSpace(key))
                throw new ArgumentException("Context keys cannot be null or whitespace.", nameof(key));

            _variables[key.Trim()] = new ContextValue(value, Format(value));
            return this;
        }

        public bool Remove(string key)
        {
            return !string.IsNullOrWhiteSpace(key) && _variables.Remove(key.Trim());
        }

        public void Clear() => _variables.Clear();

        public bool Contains(string key)
        {
            return !string.IsNullOrWhiteSpace(key) && _variables.ContainsKey(key.Trim());
        }

        public bool TryGet<T>(string key, out T value)
        {
            if (!string.IsNullOrWhiteSpace(key) &&
                _variables.TryGetValue(key.Trim(), out var entry) &&
                entry.Value is T typed)
            {
                value = typed;
                return true;
            }

            value = default;
            return false;
        }

        internal bool TryResolveStringNormalized(string key, out string value)
        {
            if (string.IsNullOrEmpty(key) ||
                !_variables.TryGetValue(key, out var entry))
            {
                value = null;
                return false;
            }

            value = entry.Formatted;
            return true;
        }

        private static string Format<T>(T value)
        {
            if (value is null)
                return string.Empty;

            if (value is string stringValue)
                return stringValue;

            return value is IFormattable formattable
                ? formattable.ToString(null, CultureInfo.InvariantCulture)
                : value.ToString();
        }

        private readonly struct ContextValue
        {
            public readonly object Value;
            public readonly string Formatted;

            public ContextValue(object value, string formatted)
            {
                Value = value;
                Formatted = formatted ?? string.Empty;
            }
        }
    }
}
