using System;

namespace IIMLib.Dialogue.Parsing
{
    public sealed class DialogueSyntax
    {
        public static DialogueSyntax Default { get; } = new DialogueSyntax();

        public string FunctionPrefix { get; }
        public string FunctionSuffix { get; }
        public string IdentifierPrefix { get; }
        public string IdentifierSuffix { get; }
        public char EscapeCharacter { get; }

        public DialogueSyntax(
            string functionPrefix = "#%{",
            string functionSuffix = "}",
            string identifierPrefix = "={",
            string identifierSuffix = "}",
            char escapeCharacter = '\\')
        {
            FunctionPrefix = ValidateMarker(functionPrefix, nameof(functionPrefix));
            FunctionSuffix = ValidateMarker(functionSuffix, nameof(functionSuffix));
            IdentifierPrefix = ValidateMarker(identifierPrefix, nameof(identifierPrefix));
            IdentifierSuffix = ValidateMarker(identifierSuffix, nameof(identifierSuffix));
            EscapeCharacter = escapeCharacter;

            if (string.Equals(FunctionPrefix, IdentifierPrefix, StringComparison.Ordinal))
                throw new ArgumentException("Function and identifier prefixes must be distinct.");

            if (FunctionPrefix[0] == EscapeCharacter ||
                FunctionSuffix[0] == EscapeCharacter ||
                IdentifierPrefix[0] == EscapeCharacter ||
                IdentifierSuffix[0] == EscapeCharacter)
            {
                throw new ArgumentException(
                    "Dialogue syntax markers cannot begin with the configured escape character.");
            }

            ValidateSuffixDoesNotShadowPrefix(FunctionSuffix, FunctionPrefix);
            ValidateSuffixDoesNotShadowPrefix(FunctionSuffix, IdentifierPrefix);
            ValidateSuffixDoesNotShadowPrefix(IdentifierSuffix, FunctionPrefix);
            ValidateSuffixDoesNotShadowPrefix(IdentifierSuffix, IdentifierPrefix);
        }

        public string CreateFunction(string identifier, string argument = null)
        {
            if (string.IsNullOrWhiteSpace(identifier))
                return string.Empty;

            identifier = identifier.Trim();
            return argument == null
                ? FunctionPrefix + identifier + FunctionSuffix
                : FunctionPrefix + identifier + "(" + argument + ")" + FunctionSuffix;
        }

        public string CreateIdentifier(string key)
        {
            if (string.IsNullOrWhiteSpace(key))
                return string.Empty;

            return IdentifierPrefix + key.Trim() + IdentifierSuffix;
        }

        internal bool RequiresCompilation(string input)
        {
            if (string.IsNullOrEmpty(input))
                return false;

            var functionLead = FunctionPrefix[0];
            var identifierLead = IdentifierPrefix[0];

            for (var i = 0; i < input.Length; i++)
            {
                var current = input[i];

                if (current == EscapeCharacter &&
                    i + 1 < input.Length &&
                    IsEscapable(input[i + 1]))
                    return true;

                if (current != functionLead && current != identifierLead)
                    continue;

                if (TryMatchToken(input, i, out _, out _, out _))
                    return true;
            }

            return false;
        }

        internal bool TryMatchToken(
            string input,
            int index,
            out DialogueTokenKind kind,
            out string prefix,
            out string suffix)
        {
            var functionMatches = StartsWithAt(input, index, FunctionPrefix);
            var identifierMatches = StartsWithAt(input, index, IdentifierPrefix);

            if (!functionMatches && !identifierMatches)
            {
                kind = DialogueTokenKind.None;
                prefix = null;
                suffix = null;
                return false;
            }

            if (functionMatches && (!identifierMatches || FunctionPrefix.Length > IdentifierPrefix.Length))
            {
                kind = DialogueTokenKind.Function;
                prefix = FunctionPrefix;
                suffix = FunctionSuffix;
                return true;
            }

            kind = DialogueTokenKind.Identifier;
            prefix = IdentifierPrefix;
            suffix = IdentifierSuffix;
            return true;
        }

        internal bool IsEscapeSequence(string input, int index, int end)
        {
            return index + 1 < end &&
                   input[index] == EscapeCharacter &&
                   IsEscapable(input[index + 1]);
        }

        internal bool IsEscapable(char value)
        {
            return value == EscapeCharacter ||
                   value == '(' ||
                   value == ')' ||
                   BeginsWith(FunctionPrefix, value) ||
                   BeginsWith(FunctionSuffix, value) ||
                   BeginsWith(IdentifierPrefix, value) ||
                   BeginsWith(IdentifierSuffix, value);
        }

        internal static bool StartsWithAt(string input, int index, string value)
        {
            if (string.IsNullOrEmpty(input) ||
                string.IsNullOrEmpty(value) ||
                index < 0 ||
                index + value.Length > input.Length)
                return false;

            for (var i = 0; i < value.Length; i++)
            {
                if (input[index + i] != value[i])
                    return false;
            }

            return true;
        }

        private static bool BeginsWith(string value, char character) =>
            !string.IsNullOrEmpty(value) && value[0] == character;

        private static string ValidateMarker(string marker, string paramName)
        {
            if (string.IsNullOrEmpty(marker))
                throw new ArgumentException("Dialogue syntax markers cannot be null or empty.", paramName);

            return marker;
        }

        private static void ValidateSuffixDoesNotShadowPrefix(
            string suffix,
            string prefix)
        {
            if (prefix.StartsWith(suffix, StringComparison.Ordinal))
            {
                throw new ArgumentException(
                    $"Dialogue suffix '{suffix}' shadows token prefix '{prefix}'. " +
                    "Choose markers that remain unambiguous when tokens are nested.");
            }
        }
    }
}
