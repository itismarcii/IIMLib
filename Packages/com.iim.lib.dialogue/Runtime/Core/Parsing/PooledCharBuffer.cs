using System;
using System.Buffers;

namespace IIMLib.Dialogue.Parsing
{
    internal struct PooledCharBuffer : IDisposable
    {
        private const int MinimumCapacity = 16;

        private char[] _buffer;
        private int _length;

        public int Length => _length;

        public ReadOnlySpan<char> WrittenSpan =>
            _buffer == null
                ? ReadOnlySpan<char>.Empty
                : new ReadOnlySpan<char>(_buffer, 0, _length);

        public PooledCharBuffer(int capacity)
        {
            _buffer = ArrayPool<char>.Shared.Rent(Math.Max(MinimumCapacity, capacity));
            _length = 0;
        }

        public void Append(char value)
        {
            EnsureCapacity(1);
            _buffer[_length++] = value;
        }

        public void Append(string value)
        {
            if (string.IsNullOrEmpty(value))
                return;

            EnsureCapacity(value.Length);
            value.AsSpan().CopyTo(new Span<char>(_buffer, _length, value.Length));
            _length += value.Length;
        }

        public void Append(ReadOnlySpan<char> value)
        {
            if (value.Length == 0)
                return;

            EnsureCapacity(value.Length);
            value.CopyTo(new Span<char>(_buffer, _length, value.Length));
            _length += value.Length;
        }

        public string ToStringValue()
        {
            if (_length == 0)
                return string.Empty;

            return new string(_buffer, 0, _length);
        }

        public void Dispose()
        {
            var buffer = _buffer;
            _buffer = null;
            _length = 0;

            if (buffer != null)
                ArrayPool<char>.Shared.Return(buffer);
        }

        private void EnsureCapacity(int additionalLength)
        {
            var required = _length + additionalLength;
            if (_buffer != null && required <= _buffer.Length)
                return;

            var newCapacity = _buffer == null
                ? Math.Max(MinimumCapacity, required)
                : Math.Max(required, _buffer.Length * 2);

            var replacement = ArrayPool<char>.Shared.Rent(newCapacity);
            if (_buffer != null)
            {
                new ReadOnlySpan<char>(_buffer, 0, _length).CopyTo(new Span<char>(replacement));
                ArrayPool<char>.Shared.Return(_buffer);
            }

            _buffer = replacement;
        }
    }
}
