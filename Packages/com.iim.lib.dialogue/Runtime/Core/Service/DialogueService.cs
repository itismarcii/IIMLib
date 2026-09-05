using System;
using System.Collections.Generic;
using IIMLib.Dialogue.Model;
using IIMLib.Dialogue.Parsing;
using IIMLib.Dialogue.Parsing.BuiltIn;

namespace IIMLib.Dialogue.Service
{
    public sealed class DialogueService : IDialogueService
    {
        private const int DefaultTemplateCacheCapacity = 256;

        private readonly Dictionary<string, FunctionEntry> _functions =
            new Dictionary<string, FunctionEntry>(StringComparer.Ordinal);

        private readonly List<ResolverEntry> _identifierResolvers = new List<ResolverEntry>();
        private readonly IDialogueFunction[] _configuredFunctions;
        private readonly IDialogueIdentifierResolver[] _configuredResolvers;
        private readonly DialogueCompiler _compiler;
        private readonly DialogueTemplateCache _templateCache;

        private long _nextRegistrationId = 1;
        private bool _initialized;

        public DialogueSyntax Syntax { get; }

        public DialogueService()
            : this(
                DialogueSyntax.Default,
                Array.Empty<IDialogueFunction>(),
                Array.Empty<IDialogueIdentifierResolver>(),
                DefaultTemplateCacheCapacity)
        {
        }

        public DialogueService(IDialogueServiceConfig config)
            : this(
                config?.Syntax ?? DialogueSyntax.Default,
                config?.Functions,
                config?.IdentifierResolvers,
                config != null
                    ? Math.Max(0, config.TemplateCacheCapacity)
                    : DefaultTemplateCacheCapacity)
        {
        }

        public DialogueService(
            DialogueSyntax syntax,
            IEnumerable<IDialogueFunction> functions = null,
            IEnumerable<IDialogueIdentifierResolver> identifierResolvers = null,
            int templateCacheCapacity = DefaultTemplateCacheCapacity)
        {
            Syntax = syntax ?? DialogueSyntax.Default;
            _configuredFunctions = ToArray(functions);
            _configuredResolvers = ToArray(identifierResolvers);
            _compiler = new DialogueCompiler(Syntax);
            _templateCache = new DialogueTemplateCache(Math.Max(0, templateCacheCapacity));

            Initialize();
        }

        public void Initialize()
        {
            if (_initialized)
                return;

            _initialized = true;

            RegisterPermanentFunction(new Addition(), replaceExisting: false);
            RegisterPermanentFunction(new Multiply(), replaceExisting: false);
            RegisterPermanentFunction(new Division(), replaceExisting: false);

            for (var i = 0; i < _configuredFunctions.Length; i++)
            {
                var function = _configuredFunctions[i];
                if (function != null)
                    RegisterPermanentFunction(function, replaceExisting: true);
            }

            for (var i = 0; i < _configuredResolvers.Length; i++)
            {
                var resolver = _configuredResolvers[i];
                if (resolver != null && !ContainsResolver(resolver))
                    _identifierResolvers.Add(new ResolverEntry(resolver, 0));
            }
        }

        public DialogueTemplate Compile(string source)
        {
            return _compiler.Compile(source);
        }

        public DialogueTemplate CompileCached(string source)
        {
            source ??= string.Empty;

            if (_templateCache.TryGet(source, out var template))
                return template;

            template = _compiler.Compile(source);
            _templateCache.Add(source, template);
            return template;
        }

        public void ClearTemplateCache() => _templateCache.Clear();

        public string Resolve(string source, DialogueContext context = null)
        {
            if (string.IsNullOrEmpty(source) || !Syntax.RequiresCompilation(source))
                return source;

            TryResolve(CompileCached(source), context, out var value, out _);
            return value;
        }

        public string Resolve(IDialogueText text, DialogueContext context = null)
        {
            if (text == null)
                return string.Empty;

            return Resolve(text.GetText(Syntax), context);
        }

        public string Resolve(DialogueTemplate template, DialogueContext context = null)
        {
            if (template == null)
                return string.Empty;

            TryResolve(template, context, out var value, out _);
            return value;
        }

        public bool TryResolve(
            string source,
            DialogueContext context,
            out string value,
            out DialogueFailure failure)
        {
            if (string.IsNullOrEmpty(source) || !Syntax.RequiresCompilation(source))
            {
                value = source;
                failure = DialogueFailure.None;
                return true;
            }

            return TryResolve(CompileCached(source), context, out value, out failure);
        }

        public bool TryResolve(
            DialogueTemplate template,
            DialogueContext context,
            out string value,
            out DialogueFailure failure)
        {
            if (template == null)
            {
                value = string.Empty;
                failure = new DialogueFailure(DialogueFailureReason.InvalidSyntax);
                return false;
            }

            return template.TryResolve(this, context, out value, out failure);
        }

        public DialogueRegistration RegisterFunction(IDialogueFunction function)
        {
            if (function == null)
                throw new ArgumentNullException(nameof(function));

            var key = NormalizeKey(function.Identifier, nameof(function));
            if (_functions.ContainsKey(key))
                throw new InvalidOperationException($"A dialogue function named '{key}' is already registered.");

            var id = NextRegistrationId();
            _functions.Add(key, new FunctionEntry(function, id));
            return new DialogueRegistration(this, id, key, null);
        }

        public DialogueRegistration RegisterIdentifierResolver(IDialogueIdentifierResolver resolver)
        {
            if (resolver == null)
                throw new ArgumentNullException(nameof(resolver));

            if (ContainsResolver(resolver))
                throw new InvalidOperationException("The dialogue identifier resolver is already registered.");

            var id = NextRegistrationId();
            _identifierResolvers.Add(new ResolverEntry(resolver, id));
            return new DialogueRegistration(this, id, null, resolver);
        }

        public bool TryEvaluateFunction(
            string identifier,
            string argument,
            DialogueContext context,
            out string value,
            out DialogueFailureReason failureReason)
        {
            if (string.IsNullOrWhiteSpace(identifier) ||
                !TryGetFunctionInternal(identifier.Trim(), out var function))
            {
                value = null;
                failureReason = DialogueFailureReason.UnknownFunction;
                return false;
            }

            if (function.TryEvaluate(
                    (argument ?? string.Empty).AsSpan(),
                    argument != null,
                    context,
                    out value,
                    out failureReason))
            {
                failureReason = DialogueFailureReason.None;
                return true;
            }

            if (failureReason == DialogueFailureReason.None)
                failureReason = DialogueFailureReason.FunctionEvaluationFailed;

            return false;
        }

        public bool TryResolveIdentifier(string key, DialogueContext context, out string value)
        {
            if (string.IsNullOrWhiteSpace(key))
            {
                value = null;
                return false;
            }

            return TryResolveIdentifierInternal(key.Trim(), context, out value);
        }

        public bool IsFunctionToken(string value) => _compiler.IsSingleFunction(value);

        public bool IsIdentifierToken(string value) => _compiler.IsSingleIdentifier(value);

        internal bool TryGetFunctionInternal(string identifier, out IDialogueFunction function)
        {
            if (!string.IsNullOrEmpty(identifier) &&
                _functions.TryGetValue(identifier, out var entry))
            {
                function = entry.Function;
                return true;
            }

            function = null;
            return false;
        }

        internal bool TryResolveIdentifierInternal(
            string key,
            DialogueContext context,
            out string value)
        {
            if (string.IsNullOrEmpty(key))
            {
                value = null;
                return false;
            }

            if (context != null && context.TryResolveStringNormalized(key, out value))
                return true;

            for (var i = _identifierResolvers.Count - 1; i >= 0; i--)
            {
                if (_identifierResolvers[i].Resolver.TryResolve(key, context, out value))
                    return true;
            }

            value = null;
            return false;
        }

        internal void Unregister(DialogueRegistration registration)
        {
            if (registration == null || registration.Id == 0)
                return;

            if (!string.IsNullOrEmpty(registration.FunctionKey))
            {
                if (_functions.TryGetValue(registration.FunctionKey, out var entry) &&
                    entry.RegistrationId == registration.Id)
                {
                    _functions.Remove(registration.FunctionKey);
                }

                return;
            }

            for (var i = _identifierResolvers.Count - 1; i >= 0; i--)
            {
                var entry = _identifierResolvers[i];
                if (entry.RegistrationId == registration.Id &&
                    ReferenceEquals(entry.Resolver, registration.Resolver))
                {
                    _identifierResolvers.RemoveAt(i);
                    return;
                }
            }
        }

        private void RegisterPermanentFunction(IDialogueFunction function, bool replaceExisting)
        {
            var key = NormalizeKey(function.Identifier, nameof(function));
            if (_functions.ContainsKey(key))
            {
                if (!replaceExisting)
                    return;

                _functions[key] = new FunctionEntry(function, 0);
                return;
            }

            _functions.Add(key, new FunctionEntry(function, 0));
        }

        private bool ContainsResolver(IDialogueIdentifierResolver resolver)
        {
            for (var i = 0; i < _identifierResolvers.Count; i++)
            {
                if (ReferenceEquals(_identifierResolvers[i].Resolver, resolver))
                    return true;
            }

            return false;
        }

        private long NextRegistrationId()
        {
            var id = _nextRegistrationId++;
            if (_nextRegistrationId <= 0)
                _nextRegistrationId = 1;

            return id;
        }

        private static string NormalizeKey(string key, string paramName)
        {
            if (string.IsNullOrWhiteSpace(key))
                throw new ArgumentException("Dialogue identifiers cannot be null or whitespace.", paramName);

            return key.Trim();
        }

        private static T[] ToArray<T>(IEnumerable<T> source)
        {
            if (source == null)
                return Array.Empty<T>();

            return source as T[] ?? new List<T>(source).ToArray();
        }

        private readonly struct FunctionEntry
        {
            public readonly IDialogueFunction Function;
            public readonly long RegistrationId;

            public FunctionEntry(IDialogueFunction function, long registrationId)
            {
                Function = function;
                RegistrationId = registrationId;
            }
        }

        private readonly struct ResolverEntry
        {
            public readonly IDialogueIdentifierResolver Resolver;
            public readonly long RegistrationId;

            public ResolverEntry(IDialogueIdentifierResolver resolver, long registrationId)
            {
                Resolver = resolver;
                RegistrationId = registrationId;
            }
        }
    }
}
