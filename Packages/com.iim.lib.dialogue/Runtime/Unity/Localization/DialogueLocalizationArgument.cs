namespace IIMLib.Dialogue.Localization
{
    public readonly struct DialogueLocalizationArgument
    {
        public string Key { get; }
        public string Value { get; }

        public DialogueLocalizationArgument(string key, string value)
        {
            Key = key ?? string.Empty;
            Value = value ?? string.Empty;
        }
    }
}
