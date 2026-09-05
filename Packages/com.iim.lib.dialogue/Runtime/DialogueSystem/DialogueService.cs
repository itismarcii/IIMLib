using System;
using System.Collections.Generic;
using System.Text;
using DialogueSystem.Util;

namespace DialogueSystem
{
    public class DialogueService : IDialogueService
    {
        private const string FUNC_PREFIX = "#%{";
        private const string FUNC_SUFFIX = "}";

        private const string IDENTIFIER_PREFIX = "={";
        private const string IDENTIFIER_SUFFIX = "}";

        private readonly struct DialogueFunctionCall
        {
            public string Identifier { get; }
            public bool HasArgument { get; }
            public string Argument { get; }

            public DialogueFunctionCall(string identifier, bool hasArgument, string argument)
            {
                Identifier = identifier;
                HasArgument = hasArgument;
                Argument = argument;
            }
        }

        private readonly Dictionary<string, IDialogueFunc> _functionCollection = new();
        private readonly List<IDialogueIdentifierDictionary> _identifierResolvers = new();

        private readonly IDialogueFunc[] _funcConfigs;
        private readonly IDialogueIdentifierDictionary[] _identifierConfigs;

        public DialogueService()
            : this(Array.Empty<IDialogueFunc>(), Array.Empty<IDialogueIdentifierDictionary>())
        {
        }

        public DialogueService(IEnumerable<IDialogueFunc> func)
            : this(func, Array.Empty<IDialogueIdentifierDictionary>())
        {
        }

        public DialogueService(IEnumerable<IDialogueIdentifierDictionary> identifiers)
            : this(Array.Empty<IDialogueFunc>(), identifiers)
        {
        }

        public DialogueService(IEnumerable<IDialogueFunc> func, IEnumerable<IDialogueIdentifierDictionary> identifiers)
        {
            _funcConfigs = func is null ? Array.Empty<IDialogueFunc>() : ToArray(func);
            _identifierConfigs = identifiers is null ? Array.Empty<IDialogueIdentifierDictionary>() : ToArray(identifiers);
        }

        public DialogueService(IDialogueServiceConfig config)
            : this(
                config?.Func ?? Array.Empty<IDialogueFunc>(),
                config?.Identifiers ?? Array.Empty<IDialogueIdentifierDictionary>())
        {
        }

        public void Initialize()
        {
            TryAddFunction(Addition.KEY, new Addition());
            TryAddFunction(Multiply.KEY, new Multiply());
            TryAddFunction(Division.KEY, new Division());

            foreach (var func in _funcConfigs)
            {
                if (func is null) continue;
                TryAddFunction(func.Identifier, func);
            }

            foreach (var identifierResolver in _identifierConfigs)
            {
                if (identifierResolver is null) continue;
                TryAddIdentifier(identifierResolver);
            }
        }

        public string FuncPrefix => FUNC_PREFIX;
        public string FuncSuffix => FUNC_SUFFIX;

        public string IdentifierPrefix => IDENTIFIER_PREFIX;
        public string IdentifierSuffix => IDENTIFIER_SUFFIX;

        public bool TryAddFunction(string key, IDialogueFunc func)
        {
            if (string.IsNullOrWhiteSpace(key) || func is null)
                return false;

            return _functionCollection.TryAdd(key, func);
        }

        public string GetFunctionString(string identifier, string key = null) =>
            _functionCollection.TryGetValue(identifier, out var func)
                ? func.CollectInfo(key)
                : string.Empty;

        public bool IsFunctionString(string s) =>
            !string.IsNullOrEmpty(s) && s.Contains(FUNC_PREFIX, StringComparison.Ordinal);

        public bool TryAddIdentifier(IDialogueIdentifierDictionary identifierDictionary)
        {
            if (identifierDictionary is null)
                return false;

            _identifierResolvers.Add(identifierDictionary);
            return true;
        }

        public string GetIdentifierString(string key)
        {
            if (string.IsNullOrWhiteSpace(key))
                return string.Empty;

            return ResolveIdentifier(key);
        }

        public bool IsIdentifierString(string s) =>
            !string.IsNullOrEmpty(s) && s.Contains(IDENTIFIER_PREFIX, StringComparison.Ordinal);

        public string ResolveDialogueString(string s)
        {
            if (string.IsNullOrEmpty(s)) return s;

            var resolvedFunctions = ResolveFunctions(s);
            var resolvedIdentifiers = ResolveIdentifiers(resolvedFunctions);

            return resolvedIdentifiers;
        }

        private string ResolveFunctions(string input)
        {
            var result = new StringBuilder(input.Length);
            var index = 0;

            while (index < input.Length)
            {
                var functionStart = input.IndexOf(FUNC_PREFIX, index, StringComparison.Ordinal);

                if (functionStart < 0)
                {
                    result.Append(input, index, input.Length - index);
                    break;
                }

                result.Append(input, index, functionStart - index);

                if (!TryFindFunctionEnd(input, functionStart, out var functionEnd))
                {
                    result.Append(input, functionStart, input.Length - functionStart);
                    break;
                }

                var innerStart = functionStart + FUNC_PREFIX.Length;
                var raw = input.Substring(innerStart, functionEnd - innerStart);

                var resolved = EvaluateFunction(raw);
                result.Append(resolved);

                index = functionEnd + FUNC_SUFFIX.Length;
            }

            return result.ToString();
        }

        private string ResolveIdentifiers(string input)
        {
            var result = new StringBuilder(input.Length);
            var index = 0;

            while (index < input.Length)
            {
                var identifierStart = input.IndexOf(IDENTIFIER_PREFIX, index, StringComparison.Ordinal);

                if (identifierStart < 0)
                {
                    result.Append(input, index, input.Length - index);
                    break;
                }

                result.Append(input, index, identifierStart - index);

                var keyStart = identifierStart + IDENTIFIER_PREFIX.Length;
                var identifierEnd = input.IndexOf(IDENTIFIER_SUFFIX, keyStart, StringComparison.Ordinal);

                if (identifierEnd < 0)
                {
                    result.Append(input, identifierStart, input.Length - identifierStart);
                    break;
                }

                var rawKey = input.Substring(keyStart, identifierEnd - keyStart).Trim();
                result.Append(ResolveIdentifier(rawKey));

                index = identifierEnd + IDENTIFIER_SUFFIX.Length;
            }

            return result.ToString();
        }

        private string ResolveIdentifier(string key)
        {
            for (var i = 0; i < _identifierResolvers.Count; i++)
            {
                var resolved = _identifierResolvers[i].Resolve(key);

                if (!string.IsNullOrEmpty(resolved)) return resolved;
            }

            return key;
        }

        private static bool TryFindFunctionEnd(string input, int functionStart, out int functionEnd)
        {
            var index = functionStart;
            var depth = 0;

            while (index < input.Length)
            {
                if (StartsWithAt(input, index, FUNC_PREFIX))
                {
                    depth++;
                    index += FUNC_PREFIX.Length;
                    continue;
                }

                if (StartsWithAt(input, index, FUNC_SUFFIX))
                {
                    depth--;

                    if (depth == 0)
                    {
                        functionEnd = index;
                        return true;
                    }

                    index += FUNC_SUFFIX.Length;
                    continue;
                }

                index++;
            }

            functionEnd = -1;
            return false;
        }

        private static bool StartsWithAt(string input, int index, string value)
        {
            if (index + value.Length > input.Length)
                return false;

            for (var i = 0; i < value.Length; i++)
            {
                if (input[index + i] != value[i])
                    return false;
            }

            return true;
        }

        private string EvaluateFunction(string raw)
        {
            var functionCall = ParseFunction(raw);

            if (!_functionCollection.TryGetValue(functionCall.Identifier, out var func)) return raw;

            if (!functionCall.HasArgument) return func.CollectInfo();

            var resolvedArgument = ResolveFunctions(functionCall.Argument);

            return func.CollectInfo(resolvedArgument);
        }

        private static DialogueFunctionCall ParseFunction(string raw)
        {
            if (string.IsNullOrWhiteSpace(raw))
                return new DialogueFunctionCall(string.Empty, false, null);

            raw = raw.Trim();

            var openParen = raw.IndexOf('(');

            if (openParen < 0)
                return new DialogueFunctionCall(raw, false, null);

            var closeParen = FindMatchingClosingParen(raw, openParen);

            if (closeParen < 0)
                return new DialogueFunctionCall(raw, false, null);

            var identifier = raw[..openParen].Trim();
            var argument = raw.Substring(openParen + 1, closeParen - openParen - 1);

            return new DialogueFunctionCall(identifier, true, argument);
        }

        private static int FindMatchingClosingParen(string input, int openParenIndex)
        {
            var depth = 0;

            for (var i = openParenIndex; i < input.Length; i++)
            {
                switch (input[i])
                {
                    case '(':
                        depth++;
                        break;

                    case ')':
                        depth--;

                        if (depth == 0)
                            return i;
                        break;
                }
            }

            return -1;
        }

        private static T[] ToArray<T>(IEnumerable<T> source)
        {
            return source as T[] ?? new List<T>(source).ToArray();
        }
    }
}