using System.Collections.Generic;
using System.Xml.Linq;

namespace Aviationexam.DependencyUpdater.Nuget.Extensions;

public static class XElementExtensions
{
    public static string? GetCondition(this XElement element)
    {
        return element.Attribute("Condition")?.Value;
    }

    public static IReadOnlyList<string> GetConditionsIncludingParents(this XElement element)
    {
        var conditions = new List<string>();

        var elementCondition = element.GetCondition();
        if (!string.IsNullOrWhiteSpace(elementCondition))
        {
            conditions.Add(elementCondition);
        }

        var parentCondition = element.Parent?.GetCondition();
        if (!string.IsNullOrWhiteSpace(parentCondition))
        {
            conditions.Add(parentCondition);
        }

        return conditions;
    }
}
