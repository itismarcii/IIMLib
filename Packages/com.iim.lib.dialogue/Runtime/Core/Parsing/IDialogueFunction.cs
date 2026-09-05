using System;
using IIMLib.Dialogue.Service;

namespace IIMLib.Dialogue.Parsing
{
    public interface IDialogueFunction
    {
        string Identifier { get; }

        bool TryEvaluate(
            ReadOnlySpan<char> argument,
            bool hasArgument,
            DialogueContext context,
            out string value,
            out DialogueFailureReason failureReason);
    }
}
