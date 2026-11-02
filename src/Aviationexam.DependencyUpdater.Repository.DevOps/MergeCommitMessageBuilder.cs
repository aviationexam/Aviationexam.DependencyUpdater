namespace Aviationexam.DependencyUpdater.Repository.DevOps;

internal static class MergeCommitMessageBuilder
{
    // Azure Repos rejects completion options whose JSON encoding exceeds 4000 characters
    // ("Completion options have exceeded the maximum encoded length"). The merge commit message is
    // counted after JSON escaping, together with the other completion option properties.
    public const int MaxCompletionOptionsEncodedLength = 4000;

    // Room for the property names and the values of the other completion options.
    public const int CompletionOptionsReservedLength = 500;

    public const int MaxEncodedMessageLength = MaxCompletionOptionsEncodedLength - CompletionOptionsReservedLength;

    public static string Build(
        string title,
        string description
    )
    {
        var message = $"{title}\n\n{description}";

        var encodedLength = 0;
        for (var i = 0; i < message.Length; i++)
        {
            encodedLength += GetMaxEncodedLength(message[i]);

            if (encodedLength > MaxEncodedMessageLength)
            {
                // Do not split a surrogate pair.
                var end = i > 0 && char.IsHighSurrogate(message[i - 1]) ? i - 1 : i;

                return message[..end];
            }
        }

        return message;
    }

    // Upper bound of the JSON-encoded length of one UTF-16 code unit. It covers serializers that
    // escape HTML-sensitive and non-ASCII characters as \uXXXX.
    internal static int GetMaxEncodedLength(char c) => c switch
    {
        '\n' or '\r' or '\t' or '\b' or '\f' or '\\' => 2,
        < ' ' or '"' or '\'' or '<' or '>' or '&' or > '~' => 6,
        _ => 1,
    };
}
