using Aviationexam.DependencyUpdater.Common;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.CommandLine.Binding;

namespace Aviationexam.DependencyUpdater;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddBinder<TService>(
        this IServiceCollection services,
        ModelBinders modelBinders,
        Func<IServiceProvider, BinderBase<TService>> implementationFactory
    ) where TService : class
    {
        modelBinders.AddModelBinder<TService, BinderBase<TService>>((invocationContext, modelBinder) =>
            {
                if (
                    modelBinder is IValueSource valueSource &&
                    valueSource.TryGetValue(modelBinder, invocationContext.BindingContext, out var boundValue) &&
                    boundValue is TService value
                )
                {
                    return value;
                }

                throw new ArgumentOutOfRangeException(nameof(modelBinder), modelBinder, null);
            }
        );

        return services.AddSingleton(
            implementationFactory
        );
    }

    public static IServiceCollection AddOptionalBinder<TService>(
        this IServiceCollection services,
        ModelBinders modelBinders,
        Func<IServiceProvider, BinderBase<Optional<TService>>> implementationFactory
    ) where TService : class
    {
        modelBinders.AddModelBinder<Optional<TService>, BinderBase<Optional<TService>>>((invocationContext, modelBinder) =>
            {
                if (
                    modelBinder is IValueSource valueSource &&
                    valueSource.TryGetValue(modelBinder, invocationContext.BindingContext, out var boundValue) &&
                    boundValue is Optional<TService> value
                )
                {
                    return value;
                }

                throw new ArgumentOutOfRangeException(nameof(modelBinder), modelBinder, null);
            }
        );

        return services.AddSingleton(
            implementationFactory
        );
    }
}
