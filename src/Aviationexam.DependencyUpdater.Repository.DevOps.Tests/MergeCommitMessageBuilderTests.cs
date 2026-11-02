using System.Linq;
using Xunit;

namespace Aviationexam.DependencyUpdater.Repository.DevOps.Tests;

public class MergeCommitMessageBuilderTests
{
    [Fact]
    public void Build_ShortMessage_ReturnsTitleAndDescription()
    {
        // Act
        var result = MergeCommitMessageBuilder.Build("Bump Foo", "Description");

        // Assert
        Assert.Equal("Bump Foo\n\nDescription", result);
    }

    [Fact]
    public void Build_LongAsciiMessage_TrimsToMaxEncodedLength()
    {
        // Arrange
        var description = new string('a', 5000);

        // Act
        var result = MergeCommitMessageBuilder.Build("Title", description);

        // Assert
        Assert.Equal(MergeCommitMessageBuilder.MaxEncodedMessageLength, GetEncodedLength(result));
        Assert.StartsWith("Title\n\naaa", result);
    }

    [Fact]
    public void Build_MessageWithEscapedCharacters_FitsEncodedLimit()
    {
        // Arrange
        // 3960 raw characters, below the old 4000-character cut, but far longer once JSON-escaped.
        var description = string.Concat(Enumerable.Repeat("- [x] \"Foo\" <bar> & ž\n", 180));

        // Act
        var result = MergeCommitMessageBuilder.Build("Title", description);

        // Assert
        Assert.True(result.Length < description.Length);
        Assert.True(GetEncodedLength(result) <= MergeCommitMessageBuilder.MaxEncodedMessageLength);
        Assert.True(
            MergeCommitMessageBuilder.MaxEncodedMessageLength + MergeCommitMessageBuilder.CompletionOptionsReservedLength
            <= MergeCommitMessageBuilder.MaxCompletionOptionsEncodedLength
        );
    }

    [Fact]
    public void Build_LimitFallsInsideSurrogatePair_DoesNotSplitPair()
    {
        // Arrange
        // "Title\n\n" encodes to 9 characters; the emoji takes 12 (two escaped code units).
        var prefixLength = MergeCommitMessageBuilder.MaxEncodedMessageLength - 9 - 6;
        var description = new string('a', prefixLength) + "\U0001F600" + "tail";

        // Act
        var result = MergeCommitMessageBuilder.Build("Title", description);

        // Assert
        Assert.False(char.IsHighSurrogate(result[^1]));
        Assert.Equal("Title\n\n".Length + prefixLength, result.Length);
    }

    [Theory]
    [InlineData('a', 1)]
    [InlineData(' ', 1)]
    [InlineData('\n', 2)]
    [InlineData('\\', 2)]
    [InlineData('"', 6)]
    [InlineData('<', 6)]
    [InlineData('\u0001', 6)]
    [InlineData('ž', 6)]
    public void GetMaxEncodedLength_ReturnsUpperBound(char c, int expected)
    {
        // Act
        var result = MergeCommitMessageBuilder.GetMaxEncodedLength(c);

        // Assert
        Assert.Equal(expected, result);
    }

    private static int GetEncodedLength(string value) => value.Sum(MergeCommitMessageBuilder.GetMaxEncodedLength);
}
