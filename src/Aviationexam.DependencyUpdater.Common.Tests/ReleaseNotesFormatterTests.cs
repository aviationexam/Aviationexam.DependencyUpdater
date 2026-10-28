using System;
using System.Linq;
using Xunit;

namespace Aviationexam.DependencyUpdater.Common.Tests;

public class ReleaseNotesFormatterTests
{
    private const string CommonNotes = """
        SharedCore Common 0.1.3215
        Changes since 0.1.3211:
        - !22542 Generate SharedCore release notes from merged PRs
        """;

    private const string ServicesNotes = """
        SharedCore Services 0.1.3215
        Changes since 0.1.3211:
        - !22600 Something else
        """;

    [Fact]
    public void NormalizeNotes_NormalizesLineEndingsAndWhitespace()
    {
        // Arrange
        var notes = "  Line one \r\n\tLine   two\t\r\n\r\n";

        // Act
        var result = ReleaseNotesFormatter.NormalizeNotes(notes);

        // Assert
        Assert.Equal("Line one\nLine two", result);
    }

    [Fact]
    public void BuildSection_DeduplicatesByNormalizedContent()
    {
        // Arrange
        var releaseNotes = new[]
        {
            new PackageReleaseNote("Aviationexam.Core.Common.Contracts", "0.1.3215", CommonNotes),
            new PackageReleaseNote("Aviationexam.Core.Common.SharedDTOs", "0.1.3215", CommonNotes.ReplaceLineEndings("\r\n") + "\r\n"),
            new PackageReleaseNote("Aviationexam.Core.Services.Abstractions", "0.1.3215", ServicesNotes),
        };

        // Act
        var result = ReleaseNotesFormatter.BuildSection(releaseNotes);

        // Assert
        var lines = result!.ReplaceLineEndings("\n").Split('\n');
        Assert.Equal(
            [
                "## Release notes",
                "",
                "**Aviationexam.Core.Common.Contracts, Aviationexam.Core.Common.SharedDTOs 0.1.3215**",
                "",
                "> SharedCore Common 0.1.3215",
                "> Changes since 0.1.3211:",
                "> - !22542 Generate SharedCore release notes from merged PRs",
                "",
                "**Aviationexam.Core.Services.Abstractions 0.1.3215**",
                "",
                "> SharedCore Services 0.1.3215",
                "> Changes since 0.1.3211:",
                "> - !22600 Something else",
            ],
            lines
        );
    }

    [Fact]
    public void BuildSection_SamePackageDifferentVersions_ListsEachVersion()
    {
        // Arrange
        var releaseNotes = new[]
        {
            new PackageReleaseNote("PackageA", "1.0.1", "Fix A"),
            new PackageReleaseNote("PackageA", "1.0.2", "Fix B"),
            new PackageReleaseNote("PackageB", "2.0.0", "Fix A"),
        };

        // Act
        var result = ReleaseNotesFormatter.BuildSection(releaseNotes);

        // Assert
        var lines = result!.ReplaceLineEndings("\n").Split('\n');
        Assert.Equal(
            [
                "## Release notes",
                "",
                "**PackageA 1.0.1; PackageB 2.0.0**",
                "",
                "> Fix A",
                "",
                "**PackageA 1.0.2**",
                "",
                "> Fix B",
            ],
            lines
        );
    }

    [Fact]
    public void BuildSection_RendersOriginalIndentationOfFirstEntry()
    {
        // Arrange
        const string notes = "- Parent  \r\n  - Nested child\r\n\r\n    var x = 1;   \r\n";
        var releaseNotes = new[]
        {
            new PackageReleaseNote("PackageA", "1.0.1", notes),
            new PackageReleaseNote("PackageB", "1.0.1", ReleaseNotesFormatter.NormalizeNotes(notes)),
        };

        // Act
        var result = ReleaseNotesFormatter.BuildSection(releaseNotes);

        // Assert
        var lines = result!.ReplaceLineEndings("\n").Split('\n');
        Assert.Equal(
            [
                "## Release notes",
                "",
                "**PackageA, PackageB 1.0.1**",
                "",
                "> - Parent",
                ">   - Nested child",
                ">",
                ">     var x = 1;",
            ],
            lines
        );
    }

    [Theory]
    [InlineData("Thanks @octocat and @my-user!", "Thanks @\u200Boctocat and @\u200Bmy-user!")]
    [InlineData("Fixes #123, see #4567.", "Fixes #\u200B123, see #\u200B4567.")]
    [InlineData("- !22542 Generate release notes", "- !22542 Generate release notes")]
    [InlineData("mail@example.com", "mail@example.com")]
    [InlineData("https://dev.azure.com/x/_git/y/branchCompare?baseVersion=GC1&targetVersion=GC2&_a=commits#1", "https://dev.azure.com/x/_git/y/branchCompare?baseVersion=GC1&targetVersion=GC2&_a=commits#1")]
    [InlineData("Heading #1 and C# 14 and #tag", "Heading #\u200B1 and C# 14 and #tag")]
    public void ScrubMentionsAndIssueReferences_Works(string input, string expected)
    {
        // Act
        var result = ReleaseNotesFormatter.ScrubMentionsAndIssueReferences(input);

        // Assert
        Assert.Equal(expected, result);
    }

    [Fact]
    public void BuildSection_ScrubsRenderedNotesButKeepsDeduplication()
    {
        // Arrange
        var releaseNotes = new[]
        {
            new PackageReleaseNote("PackageA", "1.0.1", "Fixes #12 by @octocat"),
            new PackageReleaseNote("PackageB", "1.0.1", "Fixes #12 by @octocat"),
        };

        // Act
        var result = ReleaseNotesFormatter.BuildSection(releaseNotes);

        // Assert
        var lines = result!.ReplaceLineEndings("\n").Split('\n');
        Assert.Equal(
            [
                "## Release notes",
                "",
                "**PackageA, PackageB 1.0.1**",
                "",
                "> Fixes #\u200B12 by @\u200Boctocat",
            ],
            lines
        );
    }

    [Fact]
    public void BuildSection_LongNote_IsCappedToMaxLines()
    {
        // Arrange
        var notes = string.Join('\n', Enumerable.Range(1, ReleaseNotesFormatter.MaxNoteLines + 10).Select(i => $"- change {i}"));

        // Act
        var result = ReleaseNotesFormatter.BuildSection([new PackageReleaseNote("PackageA", "1.0.1", notes)]);

        // Assert
        var noteLines = result!.ReplaceLineEndings("\n").Split('\n').Where(x => x.StartsWith('>')).ToList();
        Assert.Equal(ReleaseNotesFormatter.MaxNoteLines + 1, noteLines.Count);
        Assert.Equal($"> - change {ReleaseNotesFormatter.MaxNoteLines}", noteLines[^2]);
        Assert.Equal($"> {ReleaseNotesFormatter.NoteTruncationMarker}", noteLines[^1]);
    }

    [Fact]
    public void BuildSection_WithoutUsableNotes_ReturnsNull()
    {
        // Arrange
        var releaseNotes = new[]
        {
            new PackageReleaseNote("PackageA", "1.0.1", "   \r\n  "),
        };

        // Act
        var result = ReleaseNotesFormatter.BuildSection(releaseNotes);

        // Assert
        Assert.Null(result);
        Assert.Null(ReleaseNotesFormatter.BuildSection([]));
    }

    [Fact]
    public void AppendToDescription_WithoutSection_ReturnsDescription()
    {
        // Act
        var result = ReleaseNotesFormatter.AppendToDescription("Updates 1 package", null, 4000);

        // Assert
        Assert.Equal("Updates 1 package", result);
    }

    [Fact]
    public void AppendToDescription_WhenFits_AppendsWholeSection()
    {
        // Arrange
        var section = ReleaseNotesFormatter.BuildSection([new PackageReleaseNote("PackageA", "1.0.1", "Fix A")])!;

        // Act
        var result = ReleaseNotesFormatter.AppendToDescription("Updates 1 package", section, 4000);

        // Assert
        Assert.Equal("Updates 1 package" + Environment.NewLine + Environment.NewLine + section, result);
        Assert.DoesNotContain(ReleaseNotesFormatter.TruncationNotice, result);
    }

    [Fact]
    public void AppendToDescription_WhenTooLong_TruncatesSectionAtLineBoundaryAndAddsNotice()
    {
        // Arrange
        var description = "Updates 1 package";
        var section = ReleaseNotesFormatter.BuildSection([
            new PackageReleaseNote("PackageA", "1.0.1", string.Join('\n', Enumerable.Repeat("- a change that is long enough to matter", 50))),
        ])!;
        const int maxLength = 600;

        // Act
        var result = ReleaseNotesFormatter.AppendToDescription(description, section, maxLength);

        // Assert
        Assert.True(result.Length <= maxLength, $"Length {result.Length} exceeds {maxLength}");
        Assert.StartsWith(description + Environment.NewLine + Environment.NewLine + ReleaseNotesFormatter.SectionHeader, result);
        Assert.EndsWith(ReleaseNotesFormatter.TruncationNotice, result);

        var lines = result.ReplaceLineEndings("\n").Split('\n');
        var lastNoteLine = lines[^3];
        Assert.Equal("> - a change that is long enough to matter", lastNoteLine);
    }

    [Fact]
    public void AppendToDescription_WhenNoRoomForSection_ReturnsDescription()
    {
        // Arrange
        var description = new string('x', 3990);
        var section = ReleaseNotesFormatter.BuildSection([new PackageReleaseNote("PackageA", "1.0.1", "Fix A")])!;

        // Act
        var result = ReleaseNotesFormatter.AppendToDescription(description, section, 4000);

        // Assert
        Assert.Equal(description, result);
    }
}
