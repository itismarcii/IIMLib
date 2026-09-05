namespace IIMLib.Dialogue.Parsing
{
    public enum DialogueFailureReason
    {
        None = 0,
        InvalidSyntax,
        RecursionLimitExceeded,
        UnknownFunction,
        UnknownIdentifier,
        MissingArgument,
        InvalidArgument,
        DivisionByZero,
        FunctionEvaluationFailed
    }
}
