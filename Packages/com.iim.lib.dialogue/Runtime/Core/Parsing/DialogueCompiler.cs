using System;
using System.Collections.Generic;
using System.Text;

namespace IIMLib.Dialogue.Parsing
{
    internal sealed class DialogueCompiler
    {
        private const int MaxRecursionDepth = 64;

        private readonly DialogueSyntax _Syntax;

        public DialogueCompiler(DialogueSyntax syntax)
        {
            _Syntax = syntax ?? throw new ArgumentNullException(nameof(syntax));
        }

        public DialogueTemplate Compile(string source)
        {
            if (string.IsNullOrEmpty(source))
                return new DialogueTemplate(source, Array.Empty<DialogueNode>(), false, source);

            if (!_Syntax.RequiresCompilation(source))
                return new DialogueTemplate(source, Array.Empty<DialogueNode>(), false, source);

            var nodes = CompileRange(source, 0, source.Length, 0);
            var runtime = ContainsRuntimeNode(nodes);

            if (!runtime)
            {
                var buffer = new PooledCharBuffer(source.Length);
                try
                {
                    var failure = DialogueFailure.None;
                    for (var i = 0; i < nodes.Length; i++)
                        nodes[i].Append(null, null, ref buffer, ref failure);

                    if (!failure.HasFailure)
                    {
                        return new DialogueTemplate(
                            source,
                            Array.Empty<DialogueNode>(),
                            false,
                            buffer.ToStringValue());
                    }
                }
                finally
                {
                    buffer.Dispose();
                }
            }

            return new DialogueTemplate(source, nodes, true, null);
        }

        public bool IsSingleFunction(string value)
        {
            return IsSingleToken(value, DialogueTokenKind.Function);
        }

        public bool IsSingleIdentifier(string value)
        {
            return IsSingleToken(value, DialogueTokenKind.Identifier);
        }

        private bool IsSingleToken(string value, DialogueTokenKind expectedKind)
        {
            if (string.IsNullOrWhiteSpace(value))
                return false;

            var start = 0;
            var end = value.Length;
            TrimRange(value, ref start, ref end);

            if (!_Syntax.TryMatchToken(value, start, out var kind, out var prefix, out var suffix) ||
                kind != expectedKind)
                return false;

            if (!TryFindTokenEnd(value, start + prefix.Length, end, suffix, 0, out var tokenEnd) ||
                tokenEnd + suffix.Length != end)
                return false;

            return kind == DialogueTokenKind.Identifier
                ? IsValidIdentifierContent(value, start + prefix.Length, tokenEnd)
                : TryParseFunction(value, start, prefix, tokenEnd, out _);
        }

        private DialogueNode[] CompileRange(string source, int start, int end, int recursionDepth)
        {
            if (recursionDepth >= MaxRecursionDepth)
            {
                return new DialogueNode[]
                {
                    new InvalidDialogueNode(
                        source.Substring(start, end - start),
                        DialogueFailureReason.RecursionLimitExceeded)
                };
            }

            var nodes = new List<DialogueNode>();
            var literal = new StringBuilder();
            var index = start;

            while (index < end)
            {
                if (_Syntax.IsEscapeSequence(source, index, end))
                {
                    literal.Append(source[index + 1]);
                    index += 2;
                    continue;
                }

                if (!_Syntax.TryMatchToken(source, index, out var kind, out var prefix, out var suffix))
                {
                    literal.Append(source[index]);
                    index++;
                    continue;
                }

                FlushLiteral(nodes, literal);

                var contentStart = index + prefix.Length;
                if (!TryFindTokenEnd(source, contentStart, end, suffix, recursionDepth, out var tokenEnd))
                {
                    nodes.Add(new InvalidDialogueNode(
                        source.Substring(index, end - index),
                        DialogueFailureReason.InvalidSyntax));
                    break;
                }

                DialogueNode node;
                if (kind == DialogueTokenKind.Function)
                {
                    if (!TryParseFunction(source, index, prefix, tokenEnd, out var functionCall))
                    {
                        node = new InvalidDialogueNode(
                            source.Substring(index, tokenEnd + suffix.Length - index),
                            DialogueFailureReason.InvalidSyntax);
                    }
                    else
                    {
                        node = BuildFunctionNode(source, index, tokenEnd, suffix, functionCall, recursionDepth);
                    }
                }
                else
                {
                    node = BuildIdentifierNode(source, index, contentStart, tokenEnd, suffix, recursionDepth);
                }

                nodes.Add(node);
                index = tokenEnd + suffix.Length;
            }

            FlushLiteral(nodes, literal);
            return nodes.ToArray();
        }

        private DialogueNode BuildIdentifierNode(
            string source,
            int tokenStart,
            int contentStart,
            int contentEnd,
            string suffix,
            int recursionDepth)
        {
            var sourceToken = source.Substring(tokenStart, contentEnd + suffix.Length - tokenStart);
            TrimRange(source, ref contentStart, ref contentEnd);

            if (contentStart >= contentEnd)
                return new InvalidDialogueNode(sourceToken, DialogueFailureReason.InvalidSyntax);

            var content = CompileRange(source, contentStart, contentEnd, recursionDepth + 1);
            var template = BuildNestedTemplate(source, contentStart, contentEnd, content);

            if (!template.RequiresRuntimeResolution)
            {
                var key = (template.StaticValue ?? string.Empty).Trim();
                return key.Length == 0
                    ? new InvalidDialogueNode(sourceToken, DialogueFailureReason.InvalidSyntax)
                    : new IdentifierDialogueNode(key, sourceToken);
            }

            return new IdentifierDialogueNode(template, sourceToken);
        }

        private DialogueNode BuildFunctionNode(
            string source,
            int tokenStart,
            int tokenEnd,
            string suffix,
            FunctionCall call,
            int recursionDepth)
        {
            var sourceToken = source.Substring(tokenStart, tokenEnd + suffix.Length - tokenStart);

            if (!call.HasArgument)
                return new FunctionDialogueNode(call.Identifier, false, null, null, sourceToken);

            var argumentNodes = CompileRange(
                source,
                call.ArgumentStart,
                call.ArgumentEnd,
                recursionDepth + 1);

            var argumentTemplate = BuildNestedTemplate(
                source,
                call.ArgumentStart,
                call.ArgumentEnd,
                argumentNodes);

            if (!argumentTemplate.RequiresRuntimeResolution)
            {
                return new FunctionDialogueNode(
                    call.Identifier,
                    true,
                    argumentTemplate.StaticValue ?? string.Empty,
                    null,
                    sourceToken);
            }

            return new FunctionDialogueNode(
                call.Identifier,
                true,
                null,
                argumentTemplate,
                sourceToken);
        }

        private DialogueTemplate BuildNestedTemplate(
            string source,
            int start,
            int end,
            DialogueNode[] nodes)
        {
            var nestedSource = source.Substring(start, end - start);
            if (ContainsRuntimeNode(nodes))
                return new DialogueTemplate(nestedSource, nodes, true, null);

            var buffer = new PooledCharBuffer(nestedSource.Length);
            try
            {
                var failure = DialogueFailure.None;
                for (var i = 0; i < nodes.Length; i++)
                    nodes[i].Append(null, null, ref buffer, ref failure);

                if (failure.HasFailure)
                    return new DialogueTemplate(nestedSource, nodes, true, null);

                return new DialogueTemplate(
                    nestedSource,
                    Array.Empty<DialogueNode>(),
                    false,
                    buffer.ToStringValue());
            }
            finally
            {
                buffer.Dispose();
            }
        }

        private bool TryParseFunction(
            string source,
            int tokenStart,
            string prefix,
            int tokenEnd,
            out FunctionCall call)
        {
            var start = tokenStart + prefix.Length;
            var end = tokenEnd;
            TrimRange(source, ref start, ref end);

            if (start >= end)
            {
                call = default;
                return false;
            }

            var openParen = FindFirstFunctionParen(source, start, end);
            if (openParen < 0)
            {
                if (ContainsUnescaped(source, ')', start, end))
                {
                    call = default;
                    return false;
                }

                var identifier = source.Substring(start, end - start).Trim();
                if (identifier.Length == 0)
                {
                    call = default;
                    return false;
                }

                call = new FunctionCall(identifier, false, 0, 0);
                return true;
            }

            var identifierEnd = openParen;
            while (identifierEnd > start && char.IsWhiteSpace(source[identifierEnd - 1]))
                identifierEnd--;

            if (identifierEnd <= start)
            {
                call = default;
                return false;
            }

            var identifierValue = source.Substring(start, identifierEnd - start);
            if (!TryFindMatchingClosingParen(source, openParen, end, out var closeParen))
            {
                call = default;
                return false;
            }

            for (var i = closeParen + 1; i < end; i++)
            {
                if (!char.IsWhiteSpace(source[i]))
                {
                    call = default;
                    return false;
                }
            }

            call = new FunctionCall(identifierValue, true, openParen + 1, closeParen);
            return true;
        }

        private bool TryFindTokenEnd(
            string source,
            int contentStart,
            int limit,
            string currentSuffix,
            int recursionDepth,
            out int tokenEnd)
        {
            if (recursionDepth >= MaxRecursionDepth)
            {
                tokenEnd = -1;
                return false;
            }

            var index = contentStart;
            while (index < limit)
            {
                if (_Syntax.IsEscapeSequence(source, index, limit))
                {
                    index += 2;
                    continue;
                }

                if (DialogueSyntax.StartsWithAt(source, index, currentSuffix))
                {
                    tokenEnd = index;
                    return true;
                }

                if (_Syntax.TryMatchToken(source, index, out _, out var prefix, out var suffix))
                {
                    if (!TryFindTokenEnd(
                            source,
                            index + prefix.Length,
                            limit,
                            suffix,
                            recursionDepth + 1,
                            out var nestedEnd))
                    {
                        tokenEnd = -1;
                        return false;
                    }

                    index = nestedEnd + suffix.Length;
                    continue;
                }

                index++;
            }

            tokenEnd = -1;
            return false;
        }

        private int FindFirstFunctionParen(string source, int start, int end)
        {
            var index = start;
            while (index < end)
            {
                if (_Syntax.IsEscapeSequence(source, index, end))
                {
                    index += 2;
                    continue;
                }

                if (_Syntax.TryMatchToken(source, index, out _, out var prefix, out var suffix))
                {
                    if (!TryFindTokenEnd(source, index + prefix.Length, end, suffix, 0, out var nestedEnd))
                        return -1;

                    index = nestedEnd + suffix.Length;
                    continue;
                }

                if (source[index] == '(')
                    return index;

                index++;
            }

            return -1;
        }

        private bool TryFindMatchingClosingParen(
            string source,
            int openParen,
            int limit,
            out int closeParen)
        {
            var depth = 0;
            var index = openParen;

            while (index < limit)
            {
                if (_Syntax.IsEscapeSequence(source, index, limit))
                {
                    index += 2;
                    continue;
                }

                if (_Syntax.TryMatchToken(source, index, out _, out var prefix, out var suffix))
                {
                    if (!TryFindTokenEnd(source, index + prefix.Length, limit, suffix, 0, out var nestedEnd))
                    {
                        closeParen = -1;
                        return false;
                    }

                    index = nestedEnd + suffix.Length;
                    continue;
                }

                if (source[index] == '(')
                {
                    depth++;
                }
                else if (source[index] == ')')
                {
                    depth--;
                    if (depth == 0)
                    {
                        closeParen = index;
                        return true;
                    }
                }

                index++;
            }

            closeParen = -1;
            return false;
        }

        private bool ContainsUnescaped(string source, char value, int start, int end)
        {
            for (var i = start; i < end; i++)
            {
                if (_Syntax.IsEscapeSequence(source, i, end))
                {
                    i++;
                    continue;
                }

                if (source[i] == value)
                    return true;
            }

            return false;
        }

        private static bool IsValidIdentifierContent(string value, int start, int end)
        {
            TrimRange(value, ref start, ref end);
            return start < end;
        }

        private static bool ContainsRuntimeNode(DialogueNode[] nodes)
        {
            for (var i = 0; i < nodes.Length; i++)
            {
                if (!(nodes[i] is LiteralDialogueNode))
                    return true;
            }

            return false;
        }

        private static void FlushLiteral(List<DialogueNode> nodes, StringBuilder literal)
        {
            if (literal.Length == 0)
                return;

            nodes.Add(new LiteralDialogueNode(literal.ToString()));
            literal.Clear();
        }

        private static void TrimRange(string source, ref int start, ref int end)
        {
            while (start < end && char.IsWhiteSpace(source[start]))
                start++;

            while (end > start && char.IsWhiteSpace(source[end - 1]))
                end--;
        }

        private readonly struct FunctionCall
        {
            public readonly string Identifier;
            public readonly bool HasArgument;
            public readonly int ArgumentStart;
            public readonly int ArgumentEnd;

            public FunctionCall(string identifier, bool hasArgument, int argumentStart, int argumentEnd)
            {
                Identifier = identifier;
                HasArgument = hasArgument;
                ArgumentStart = argumentStart;
                ArgumentEnd = argumentEnd;
            }
        }
    }
}
