using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using ZLinq;

namespace Aviationexam.DependencyUpdater.Common;

public static partial class ReleaseNotesFormatter
{
    public const string SectionHeader = "## Release notes";
    public const string TruncationNotice = "_Release notes were truncated to fit the pull request description limit._";

    [GeneratedRegex(@"[ \t\f\v]+", RegexOptions.None, matchTimeoutMilliseconds: 100)]
    private static partial Regex HorizontalWhitespaceRegexFactory();

    private static readonly Regex HorizontalWhitespace = HorizontalWhitespaceRegexFactory();

    [GeneratedRegex(@"(?<![\w/.-])@(?=[A-Za-z0-9]+(?:-[A-Za-z0-9]+)*)", RegexOptions.None, matchTimeoutMilliseconds: 100)]
    private static partial Regex MentionRegexFactory();

    private static readonly Regex Mention = MentionRegexFactory();

    [GeneratedRegex(@"(?<![\w/&=?.-])#(?=\d+)", RegexOptions.None, matchTimeoutMilliseconds: 100)]
    private static partial Regex IssueReferenceRegexFactory();

    private static readonly Regex IssueReference = IssueReferenceRegexFactory();

    private const string ZeroWidthSpace = "\u200B";

    public static string NormalizeNotes(
        string notes
    )
    {
        var lines = notes
            .ReplaceLineEndings("\n")
            .Split('\n')
            .AsValueEnumerable()
            .Select(line => HorizontalWhitespace.Replace(line, " ").Trim())
            .ToList();

        return string.Join('\n', lines).Trim('\n');
    }

    // Keeps indentation so nested lists and code blocks survive, only line endings and trailing whitespace are unified
    private static string CleanNotes(
        string notes
    )
    {
        var lines = notes
            .ReplaceLineEndings("\n")
            .Split('\n')
            .AsValueEnumerable()
            .Select(line => line.TrimEnd())
            .ToList();

        return ScrubMentionsAndIssueReferences(string.Join('\n', lines).Trim('\n'));
    }

    // Release notes come from other repositories; "@user" and "#123" must not notify people or link issues in the target repository
    public static string ScrubMentionsAndIssueReferences(
        string text
    )
    {
        var scrubbed = Mention.Replace(text, "@" + ZeroWidthSpace);

        return IssueReference.Replace(scrubbed, "#" + ZeroWidthSpace);
    }

    public static string? BuildSection(
        IReadOnlyCollection<PackageReleaseNote> releaseNotes
    )
    {
        var groups = new Dictionary<string, List<PackageReleaseNote>>(StringComparer.Ordinal);
        var order = new List<string>();
        var representatives = new Dictionary<string, string>(StringComparer.Ordinal);

        foreach (var releaseNote in releaseNotes)
        {
            var normalized = NormalizeNotes(releaseNote.Notes);
            if (normalized.Length == 0)
            {
                continue;
            }

            if (!groups.TryGetValue(normalized, out var group))
            {
                group = [];
                groups.Add(normalized, group);
                order.Add(normalized);
                representatives.Add(normalized, CleanNotes(releaseNote.Notes));
            }

            if (!group.AsValueEnumerable().Any(x => x.PackageName == releaseNote.PackageName && x.Version == releaseNote.Version))
            {
                group.Add(releaseNote);
            }
        }

        if (order.Count == 0)
        {
            return null;
        }

        var stringBuilder = new StringBuilder();
        stringBuilder.AppendLine(SectionHeader);

        foreach (var normalized in order)
        {
            stringBuilder.AppendLine();
            stringBuilder.Append("**").Append(FormatAppliesTo(groups[normalized])).AppendLine("**");
            stringBuilder.AppendLine();

            foreach (var line in representatives[normalized].Split('\n'))
            {
                stringBuilder.Append('>');
                if (line.Length > 0)
                {
                    stringBuilder.Append(' ').Append(line);
                }

                stringBuilder.AppendLine();
            }
        }

        return stringBuilder.ToString().TrimEnd();
    }

    public static string AppendToDescription(
        string description,
        string? releaseNotesSection,
        int maxLength
    )
    {
        if (string.IsNullOrEmpty(releaseNotesSection))
        {
            return description;
        }

        var separator = Environment.NewLine + Environment.NewLine;
        var full = description + separator + releaseNotesSection;
        if (full.Length <= maxLength)
        {
            return full;
        }

        var suffix = separator + TruncationNotice;
        var available = maxLength - description.Length - separator.Length - suffix.Length;
        if (available < SectionHeader.Length)
        {
            return description;
        }

        var cut = releaseNotesSection.LastIndexOf('\n', available - 1);
        var truncated = cut > 0
            ? releaseNotesSection[..cut]
            : releaseNotesSection[..available];

        return description + separator + truncated.TrimEnd() + suffix;
    }

    private static string FormatAppliesTo(
        IReadOnlyCollection<PackageReleaseNote> group
    ) => group
        .AsValueEnumerable()
        .GroupBy(x => x.Version)
        .Select(x => $"{x.AsValueEnumerable().Select(n => n.PackageName).JoinToString(", ")} {x.Key}")
        .JoinToString("; ");
}
