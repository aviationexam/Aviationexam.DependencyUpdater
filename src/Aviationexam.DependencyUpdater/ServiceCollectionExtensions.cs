using Aviationexam.DependencyUpdater.Common;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.CommandLine;

namespace Aviationexam.DependencyUpdater;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddBinder<TValue>(
        this IServiceCollection services,
        ParseResult parseResult,
        IBinder<TValue> binder
    ) where TValue : class => services
        .AddSingleton<TValue>(_ => binder.CreateValue(parseResult));

    public static IServiceCollection AddBinder<TValue>(
        this IServiceCollection services,
        ParseResult parseResult,
        Func<IServiceProvider, IBinder<TValue>> implementationFactory
    ) where TValue : class => services.AddSingleton<TValue>(serviceProvider => implementationFactory(serviceProvider)
        .CreateValue(parseResult)
    );

    public static IServiceCollection AddOptionalBinder<TValue>(
        this IServiceCollection services,
        ParseResult parseResult,
        IBinder<Optional<TValue>> binder
    ) where TValue : class => services
        .AddSingleton<Optional<TValue>>(_ => binder.CreateValue(parseResult));

    public static IServiceCollection AddOptionalBinder<TValue>(
        this IServiceCollection services,
        ParseResult parseResult,
        Func<IServiceProvider, IBinder<Optional<TValue>>> implementationFactory
    ) where TValue : class => services
        .AddSingleton<Optional<TValue>>(serviceProvider => implementationFactory(serviceProvider)
            .CreateValue(parseResult)
        );
}
