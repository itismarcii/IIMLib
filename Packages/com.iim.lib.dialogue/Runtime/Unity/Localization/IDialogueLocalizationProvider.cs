using System;
using IIMLib.Core;

namespace IIMLib.Dialogue.Localization
{
    public interface IDialogueLocalizationProvider : IService
    {
        string Get(string key);

        string Get(
            string key,
            ReadOnlySpan<DialogueLocalizationArgument> arguments);
    }
}
