using System.IO;

namespace Aviationexam.DependencyUpdater.Nuget.Tests;

public static class StreamExtensions
{
    public static Stream AsStream(
        this string value
        ) => new MemoryStream(System.Text.Encoding.UTF8.GetBytes(value));
}
