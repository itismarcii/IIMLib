using System;
using System.Globalization;
using IIMLib.Dialogue.Service;

namespace IIMLib.Dialogue.Parsing.BuiltIn
{
    public sealed class Division : IDialogueFunction
    {
        public const string Key = "Division";
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

            if (!NumericExpressionParser.TryDivide(argument, out var result, out var divisionByZero))
            {
                value = null;
                failureReason = divisionByZero
                    ? DialogueFailureReason.DivisionByZero
                    : DialogueFailureReason.InvalidArgument;
                return false;
            }

            value = result.ToString(CultureInfo.InvariantCulture);
            failureReason = DialogueFailureReason.None;
            return true;
        }
    }
}
