using System;
using System.Collections.Generic;

namespace Aviationexam.DependencyUpdater.Nuget.Services;

public sealed class NugetReleaseLabelComparer : IComparer<IReadOnlyCollection<string>>
{
    public static NugetReleaseLabelComparer Instance { get; } = new();

    private NugetReleaseLabelComparer()
    {
    }

    public int Compare(IReadOnlyCollection<string>? x, IReadOnlyCollection<string>? y)
    {
        if (ReferenceEquals(x, y)) return 0;
        if (x is null) return -1;
        if (y is null) return 1;

        using var xEnumerator = x.GetEnumerator();
        using var yEnumerator = y.GetEnumerator();

        while (true)
        {
            var hasNextX = xEnumerator.MoveNext();
            var hasNextY = yEnumerator.MoveNext();

            switch (hasNextX, hasNextY)
            {
                case (false, false): return 0;
                case (false, _): return -1;
                case (_, false): return 1;
            }

            var labelX = xEnumerator.Current;
            var labelY = yEnumerator.Current;

            var isNumericX = int.TryParse(labelX, out var numericX);
            var isNumericY = int.TryParse(labelY, out var numericY);

            switch (isNumericX, isNumericY)
            {
                case (true, true) when numericX.CompareTo(numericY) == 0: break;
                case (true, true): return numericX.CompareTo(numericY);
                case (true, _): return -1;
                case (_, true): return 1;
            }

            if (
                string.Compare(labelX, labelY, StringComparison.Ordinal) is var strComparison
                && strComparison != 0
            )
            {
                return strComparison;
            }
        }
    }
}
