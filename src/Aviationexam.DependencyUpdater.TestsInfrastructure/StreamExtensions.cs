using System.IO;

namespace Aviationexam.DependencyUpdater.TestsInfrastructure;

public static class StreamExtensions
{
    public static Stream AsStream(
        this string content, string? fileName = null
    )
    {
        if (fileName is not null)
        {
            File.WriteAllText(fileName, content);
        }

        return new MemoryStream(System.Text.Encoding.UTF8.GetBytes(content));
    }
}
