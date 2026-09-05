namespace IIMLib.Dialogue.Parsing
{
    public readonly struct DialogueFailure
    {
        public static DialogueFailure None => default;

        public DialogueFailureReason Reason { get; }
        public string Token { get; }
        public string Identifier { get; }

        public bool HasFailure => Reason != DialogueFailureReason.None;

        public DialogueFailure(
            DialogueFailureReason reason,
            string token = null,
            string identifier = null)
        {
            Reason = reason;
            Token = token;
            Identifier = identifier;
        }

        public override string ToString()
        {
            if (!HasFailure)
                return "No dialogue failure.";

            if (!string.IsNullOrEmpty(Identifier))
                return $"{Reason}: {Identifier} ({Token})";

            return string.IsNullOrEmpty(Token)
                ? Reason.ToString()
                : $"{Reason}: {Token}";
        }
    }
}
