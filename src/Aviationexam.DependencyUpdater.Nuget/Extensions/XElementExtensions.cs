using System.Xml.Linq;

namespace Aviationexam.DependencyUpdater.Nuget.Extensions;

public static class XElementExtensions
{
    public static string? GetCondition(this XElement element)
    {
        return element.Attribute("Condition")?.Value;
    }

    public static string? GetConditionIncludingParent(this XElement element)
    {
        return element.GetCondition() ?? element.Parent?.GetCondition();
    }
}
