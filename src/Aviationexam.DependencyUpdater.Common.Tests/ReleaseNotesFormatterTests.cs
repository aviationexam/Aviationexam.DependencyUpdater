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
