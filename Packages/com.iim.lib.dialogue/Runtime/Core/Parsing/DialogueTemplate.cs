using System;
using IIMLib.Dialogue.Service;

namespace IIMLib.Dialogue.Parsing
{
    public sealed class DialogueTemplate
    {
        internal readonly DialogueNode[] Nodes;

        public string Source { get; }
        public bool RequiresRuntimeResolution { get; }
        public string StaticValue { get; }

        internal DialogueTemplate(
            string source,
            DialogueNode[] nodes,
            bool requiresRuntimeResolution,
            string staticValue)
        {
            Source = source ?? string.Empty;
            Nodes = nodes ?? Array.Empty<DialogueNode>();
            RequiresRuntimeResolution = requiresRuntimeResolution;
            StaticValue = staticValue;
        }

        internal bool TryResolve(
            DialogueService service,
            DialogueContext context,
            out string value,
            out DialogueFailure failure)
        {
            if (!RequiresRuntimeResolution)
            {
                value = StaticValue ?? Source;
                failure = DialogueFailure.None;
                return true;
            }

            failure = DialogueFailure.None;
            
            if (Nodes.Length == 1)
                return Nodes[0].TryResolveDirect(service, context, out value, ref failure);

            var output = new PooledCharBuffer(Source.Length);
            try
            {
                var success = TryAppendTo(service, context, ref output, out failure);
                value = output.ToStringValue();
                return success;
            }
            finally
            {
                output.Dispose();
            }
        }

        internal bool TryAppendTo(
            DialogueService service,
            DialogueContext context,
            ref PooledCharBuffer output,
            out DialogueFailure failure)
        {
            failure = DialogueFailure.None;

            if (!RequiresRuntimeResolution)
            {
                output.Append(StaticValue ?? Source);
                return true;
            }

            var success = true;
            for (var i = 0; i < Nodes.Length; i++)
            {
                if (!Nodes[i].Append(service, context, ref output, ref failure))
                    success = false;
            }

            return success;
        }
    }

    internal abstract class DialogueNode
    {
        public abstract bool TryResolveDirect(
            DialogueService service,
            DialogueContext context,
            out string value,
            ref DialogueFailure failure);

        public bool Append(
            DialogueService service,
            DialogueContext context,
            ref PooledCharBuffer output,
            ref DialogueFailure failure)
        {
            var success = TryResolveDirect(service, context, out var value, ref failure);
            output.Append(value);
            return success;
        }

        protected static void SetFirstFailure(
            ref DialogueFailure target,
            DialogueFailure candidate)
        {
            if (!target.HasFailure && candidate.HasFailure)
                target = candidate;
        }
    }

    internal sealed class LiteralDialogueNode : DialogueNode
    {
        private readonly string _value;

        public LiteralDialogueNode(string value)
        {
            _value = value ?? string.Empty;
        }

        public override bool TryResolveDirect(
            DialogueService service,
            DialogueContext context,
            out string value,
            ref DialogueFailure failure)
        {
            value = _value;
            return true;
        }
    }

    internal sealed class InvalidDialogueNode : DialogueNode
    {
        private readonly string _source;
        private readonly DialogueFailureReason _reason;

        public InvalidDialogueNode(string source, DialogueFailureReason reason)
        {
            _source = source ?? string.Empty;
            _reason = reason;
        }

        public override bool TryResolveDirect(
            DialogueService service,
            DialogueContext context,
            out string value,
            ref DialogueFailure failure)
        {
            SetFirstFailure(
                ref failure,
                new DialogueFailure(_reason, _source));

            value = _source;
            return false;
        }
    }

    internal sealed class IdentifierDialogueNode : DialogueNode
    {
        private readonly string _staticKey;
        private readonly DialogueTemplate _dynamicKey;
        private readonly string _sourceToken;

        public IdentifierDialogueNode(string key, string sourceToken)
        {
            _staticKey = key;
            _sourceToken = sourceToken;
        }

        public IdentifierDialogueNode(DialogueTemplate dynamicKey, string sourceToken)
        {
            _dynamicKey = dynamicKey;
            _sourceToken = sourceToken;
        }

        public override bool TryResolveDirect(
            DialogueService service,
            DialogueContext context,
            out string value,
            ref DialogueFailure failure)
        {
            string key;

            if (_dynamicKey != null)
            {
                var keyBuffer = new PooledCharBuffer(_dynamicKey.Source.Length);
                try
                {
                    if (!_dynamicKey.TryAppendTo(
                            service,
                            context,
                            ref keyBuffer,
                            out var nestedFailure))
                    {
                        SetFirstFailure(ref failure, nestedFailure);
                        value = _sourceToken;
                        return false;
                    }

                    key = CreateTrimmedString(keyBuffer.WrittenSpan);
                }
                finally
                {
                    keyBuffer.Dispose();
                }
            }
            else
            {
                key = _staticKey;
            }

            if (string.IsNullOrEmpty(key) ||
                !service.TryResolveIdentifierInternal(key, context, out value))
            {
                SetFirstFailure(
                    ref failure,
                    new DialogueFailure(
                        DialogueFailureReason.UnknownIdentifier,
                        _sourceToken,
                        key));

                value = _sourceToken;
                return false;
            }

            value ??= string.Empty;
            return true;
        }

        private static string CreateTrimmedString(ReadOnlySpan<char> value)
        {
            var start = 0;
            var end = value.Length;

            while (start < end && char.IsWhiteSpace(value[start]))
                start++;

            while (end > start && char.IsWhiteSpace(value[end - 1]))
                end--;

            return start >= end
                ? string.Empty
                : value.Slice(start, end - start).ToString();
        }
    }

    internal sealed class FunctionDialogueNode : DialogueNode
    {
        private readonly string _identifier;
        private readonly bool _hasArgument;
        private readonly string _staticArgument;
        private readonly DialogueTemplate _dynamicArgument;
        private readonly string _sourceToken;

        public FunctionDialogueNode(
            string identifier,
            bool hasArgument,
            string staticArgument,
            DialogueTemplate dynamicArgument,
            string sourceToken)
        {
            _identifier = identifier;
            _hasArgument = hasArgument;
            _staticArgument = staticArgument;
            _dynamicArgument = dynamicArgument;
            _sourceToken = sourceToken;
        }

        public override bool TryResolveDirect(
            DialogueService service,
            DialogueContext context,
            out string value,
            ref DialogueFailure failure)
        {
            if (!service.TryGetFunctionInternal(_identifier, out var function))
            {
                SetFirstFailure(
                    ref failure,
                    new DialogueFailure(
                        DialogueFailureReason.UnknownFunction,
                        _sourceToken,
                        _identifier));

                value = _sourceToken;
                return false;
            }

            if (!_hasArgument)
            {
                return Evaluate(
                    function,
                    ReadOnlySpan<char>.Empty,
                    false,
                    context,
                    out value,
                    ref failure);
            }

            if (_dynamicArgument == null)
            {
                return Evaluate(
                    function,
                    (_staticArgument ?? string.Empty).AsSpan(),
                    true,
                    context,
                    out value,
                    ref failure);
            }

            var argumentBuffer = new PooledCharBuffer(_dynamicArgument.Source.Length);
            try
            {
                if (!_dynamicArgument.TryAppendTo(
                        service,
                        context,
                        ref argumentBuffer,
                        out var nestedFailure))
                {
                    SetFirstFailure(ref failure, nestedFailure);
                    value = _sourceToken;
                    return false;
                }

                return Evaluate(
                    function,
                    argumentBuffer.WrittenSpan,
                    true,
                    context,
                    out value,
                    ref failure);
            }
            finally
            {
                argumentBuffer.Dispose();
            }
        }

        private bool Evaluate(
            IDialogueFunction function,
            ReadOnlySpan<char> argument,
            bool hasArgument,
            DialogueContext context,
            out string value,
            ref DialogueFailure failure)
        {
            if (function.TryEvaluate(
                    argument,
                    hasArgument,
                    context,
                    out value,
                    out var failureReason))
            {
                value ??= string.Empty;
                return true;
            }

            if (failureReason == DialogueFailureReason.None)
                failureReason = DialogueFailureReason.FunctionEvaluationFailed;

            SetFirstFailure(
                ref failure,
                new DialogueFailure(
                    failureReason,
                    _sourceToken,
                    _identifier));

            value = _sourceToken;
            return false;
        }
    }
}
