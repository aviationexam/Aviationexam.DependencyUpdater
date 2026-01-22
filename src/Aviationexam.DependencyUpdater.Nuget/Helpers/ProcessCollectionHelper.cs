using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Aviationexam.DependencyUpdater.Nuget.Helpers;

public static class ProcessCollectionHelper
{
    public static
#if DEBUG
        async
#endif
        Task ForEachAsync<TSource>(IEnumerable<TSource> source, ParallelOptions parallelOptions, Func<TSource, CancellationToken, ValueTask> body)
    {
#if DEBUG
        foreach (var item in source)
        {
            await body(item, parallelOptions.CancellationToken);
        }
#else
        return Parallel.ForEachAsync(
            source,
            parallelOptions,
            body
        );
#endif
    }
}
