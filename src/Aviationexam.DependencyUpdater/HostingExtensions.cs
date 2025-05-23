using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.CommandLine.Builder;
using System.Linq;

namespace Aviationexam.DependencyUpdater;

public static class HostingExtensions
{
    private const string ConfigurationDirectiveName = "config";

    public static CommandLineBuilder UseHost(
        this CommandLineBuilder builder,
        Func<string[], Action<IConfigurationBuilder>, HostApplicationBuilder> hostBuilderFactory,
        Action<HostApplicationBuilder, ModelBinders>? configureHost = null
    ) => builder.AddMiddleware(async (context, next) =>
    {
        var argsRemaining = context.ParseResult.UnparsedTokens.ToArray();

        var hostBuilder = hostBuilderFactory(
            argsRemaining,
            config => config.AddCommandLineDirectives(context.ParseResult, ConfigurationDirectiveName)
        );

        var modelBinders = new ModelBinders();
        configureHost?.Invoke(hostBuilder, modelBinders);

        var uniqueServiceTypes = hostBuilder.Services.Select(x => x.ServiceType).ToList();

        foreach (var ((modelType, modelBinderType), createModel) in modelBinders.Binders)
        {
            hostBuilder.Services.AddSingleton(modelType, serviceProvider =>
            {
                var binder = serviceProvider.GetRequiredService(modelBinderType);

                return createModel(context, binder);
            });

            uniqueServiceTypes.Add(modelType);
            uniqueServiceTypes.Remove(modelBinderType);
        }

        using var host = hostBuilder.Build();

        context.BindingContext.AddService<IServiceProvider>(
            // ReSharper disable once AccessToDisposedClosure
            _ => host.Services
        );
        context.BindingContext.AddService(
            // ReSharper disable once AccessToDisposedClosure
            _ => context.GetCancellationToken()
        );

        foreach (var serviceType in uniqueServiceTypes)
        {
            context.BindingContext.AddService(
                serviceType,
                // ReSharper disable once AccessToDisposedClosure
                _ => host.Services.GetRequiredService(serviceType)
            );
        }

        await host.StartAsync();

        await next(context);

        await host.StopAsync();
    });
}
