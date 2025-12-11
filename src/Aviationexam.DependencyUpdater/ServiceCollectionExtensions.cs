using Aviationexam.DependencyUpdater.Common;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.CommandLine;

namespace Aviationexam.DependencyUpdater;

public static class ServiceCollectionExtensions
{
    extension(IServiceCollection services)
    {
        public IServiceCollection AddBinder<TValue>(ParseResult parseResult,
            IBinder<TValue> binder
        ) where TValue : class => services
            .AddSingleton<TValue>(_ => binder.CreateValue(parseResult));

        public IServiceCollection AddBinder<TValue>(ParseResult parseResult,
            Func<IServiceProvider, IBinder<TValue>> implementationFactory
        ) where TValue : class => services.AddSingleton<TValue>(serviceProvider => implementationFactory(serviceProvider)
            .CreateValue(parseResult)
        );

        public IServiceCollection AddOptionalBinder<TValue>(ParseResult parseResult,
            IBinder<Optional<TValue>> binder
        ) where TValue : class => services
            .AddSingleton<Optional<TValue>>(_ => binder.CreateValue(parseResult));

        public IServiceCollection AddOptionalBinder<TValue>(ParseResult parseResult,
            Func<IServiceProvider, IBinder<Optional<TValue>>> implementationFactory
        ) where TValue : class => services
            .AddSingleton<Optional<TValue>>(serviceProvider => implementationFactory(serviceProvider)
                .CreateValue(parseResult)
            );
    }
}
