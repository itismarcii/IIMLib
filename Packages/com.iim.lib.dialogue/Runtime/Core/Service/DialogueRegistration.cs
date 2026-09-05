using System;
using IIMLib.Dialogue.Parsing;

namespace IIMLib.Dialogue.Service
{
    public sealed class DialogueRegistration : IDisposable
    {
        private DialogueService _owner;
        private string _functionKey;
        private IDialogueIdentifierResolver _resolver;

        internal long Id { get; }
        internal string FunctionKey => _functionKey;
        internal IDialogueIdentifierResolver Resolver => _resolver;

        internal DialogueRegistration(
            DialogueService owner,
            long id,
            string functionKey,
            IDialogueIdentifierResolver resolver)
        {
            _owner = owner;
            Id = id;
            _functionKey = functionKey;
            _resolver = resolver;
        }

        public bool IsDisposed => _owner == null;

        public void Dispose()
        {
            var owner = _owner;
            if (owner == null)
                return;

            _owner = null;
            owner.Unregister(this);

            _functionKey = null;
            _resolver = null;
        }
    }
}
