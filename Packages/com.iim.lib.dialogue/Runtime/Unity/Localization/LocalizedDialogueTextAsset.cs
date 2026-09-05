using IIMLib.Core;
using IIMLib.Dialogue.Authoring;
using IIMLib.Dialogue.Parsing;
using UnityEngine;

namespace IIMLib.Dialogue.Localization
{
    [CreateAssetMenu(menuName = "IIM/Dialogue/Localized Text", fileName = "LocalizedDialogueText")]
    public sealed class LocalizedDialogueTextAsset : AbstractDialogueTextAsset
    {
        [SerializeField] private string _key;
        [SerializeField] private DialogueLocalizationParameter[] _parameters;

        private DialogueLocalizationArgument[] _cachedArguments;
        private long[] _cachedArgumentVersions;
        private DialogueSyntax _cachedSyntax;

        protected override string GetTextString(DialogueSyntax syntax)
        {
            if (string.IsNullOrWhiteSpace(_key))
                return string.Empty;

            if (!ServiceLocator.TryGet<IDialogueLocalizationProvider>(out var localization))
                return _key;

            if (_parameters == null || _parameters.Length == 0)
                return localization.Get(_key);

            EnsureArgumentBuffers();

            var syntaxChanged = !ReferenceEquals(_cachedSyntax, syntax);

            for (var i = 0; i < _parameters.Length; i++)
            {
                var parameter = _parameters[i];
                var version = parameter.Value != null
                    ? parameter.Value.GetContentVersion()
                    : 0;

                if (!syntaxChanged && _cachedArgumentVersions[i] == version)
                    continue;

                _cachedArguments[i] = new DialogueLocalizationArgument(
                    parameter.Key,
                    parameter.Value != null
                        ? parameter.Value.GetMethodString(syntax)
                        : string.Empty);

                _cachedArgumentVersions[i] = version;
            }

            _cachedSyntax = syntax;
            return localization.Get(_key, _cachedArguments);
        }

        private void EnsureArgumentBuffers()
        {
            if (_cachedArguments != null &&
                _cachedArgumentVersions != null &&
                _cachedArguments.Length == _parameters.Length &&
                _cachedArgumentVersions.Length == _parameters.Length)
                return;

            _cachedArguments = new DialogueLocalizationArgument[_parameters.Length];
            _cachedArgumentVersions = new long[_parameters.Length];
            _cachedSyntax = null;

            for (var i = 0; i < _cachedArgumentVersions.Length; i++)
                _cachedArgumentVersions[i] = long.MinValue;
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            _cachedArguments = null;
            _cachedArgumentVersions = null;
            _cachedSyntax = null;
        }
#endif
    }
}
