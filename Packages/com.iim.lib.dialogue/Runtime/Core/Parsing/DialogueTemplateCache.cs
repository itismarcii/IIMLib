using System;
using System.Collections.Generic;

namespace IIMLib.Dialogue.Parsing
{
    internal sealed class DialogueTemplateCache
    {
        private readonly int _capacity;
        private readonly Dictionary<string, LinkedListNode<Entry>> _entries;
        private readonly LinkedList<Entry> _lru;

        public DialogueTemplateCache(int capacity)
        {
            _capacity = Math.Max(0, capacity);
            _entries = new Dictionary<string, LinkedListNode<Entry>>(
                Math.Max(0, _capacity),
                StringComparer.Ordinal);
            _lru = new LinkedList<Entry>();
        }

        public bool TryGet(string source, out DialogueTemplate template)
        {
            if (_capacity == 0 || !_entries.TryGetValue(source, out var node))
            {
                template = null;
                return false;
            }

            _lru.Remove(node);
            _lru.AddFirst(node);
            template = node.Value.Template;
            return true;
        }

        public void Add(string source, DialogueTemplate template)
        {
            if (_capacity == 0 || source == null || template == null)
                return;

            if (_entries.TryGetValue(source, out var existing))
            {
                existing.Value = new Entry(source, template);
                _lru.Remove(existing);
                _lru.AddFirst(existing);
                return;
            }

            var node = new LinkedListNode<Entry>(new Entry(source, template));
            _lru.AddFirst(node);
            _entries.Add(source, node);

            if (_entries.Count <= _capacity)
                return;

            var last = _lru.Last;
            if (last == null)
                return;

            _lru.RemoveLast();
            _entries.Remove(last.Value.Source);
        }

        public void Clear()
        {
            _entries.Clear();
            _lru.Clear();
        }

        private readonly struct Entry
        {
            public string Source { get; }
            public DialogueTemplate Template { get; }

            public Entry(string source, DialogueTemplate template)
            {
                Source = source;
                Template = template;
            }
        }
    }
}
