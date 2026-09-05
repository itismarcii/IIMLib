using System;
using System.Globalization;
using IIMLib.Dialogue.Service;

namespace IIMLib.Dialogue.Parsing.BuiltIn
{
    public sealed class Multiply : IDialogueFunction
    {
        public const string Key = "Multiply";
        public string Identifier => Key;

        public bool TryEvaluate(
            ReadOnlySpan<char> argument,
            bool hasArgument,
            DialogueContext context,
            out string value,
            out DialogueFailureReason failureReason)
        {
            if (!hasArgument)
            {
                value = null;
                failureReason = DialogueFailureReason.MissingArgument;
                return false;
            }

            if (!NumericExpressionParser.TryMultiply(argument, out var result))
            {
                value = null;
                failureReason = DialogueFailureReason.InvalidArgument;
                return false;
            }

            value = result.ToString(CultureInfo.InvariantCulture);
            failureReason = DialogueFailureReason.None;
            return true;
        }
    }
}
