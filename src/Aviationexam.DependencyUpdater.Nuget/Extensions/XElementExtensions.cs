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

        var current = element;
        while (current is not null)
        {
            var condition = current.GetCondition();
            if (!string.IsNullOrWhiteSpace(condition))
            {
                conditions.Add(condition);
            }

            current = current.Parent;
        }

        return conditions;
    }
}
