using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.CommandLine;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using System.Threading.Tasks;
using ZLinq;

namespace Aviationexam.DependencyUpdater;

public static class HostingExtensions
{
    public static async Task<int> ExecuteCommandHandlerAsync<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] TService>(
        this ParseResult parseResult,
        Func<string[], Action<IConfigurationBuilder>, HostApplicationBuilder> hostBuilderFactory,
        Action<IServiceCollection, ParseResult> addConfigurations,
        CancellationToken cancellationToken
    ) where TService : class, ICommandHandler
    {
        var argsRemaining = parseResult.UnmatchedTokens.AsValueEnumerable().ToArray();

        var hostBuilder = hostBuilderFactory(
            argsRemaining,
            _ => { }
        );

        addConfigurations(hostBuilder.Services, parseResult);

        hostBuilder.Services.AddSingleton<TService>();

        using var host = hostBuilder.Build();

        await host.StartAsync(cancellationToken);

        var service = host.Services.GetRequiredService<TService>();
        var exitCode = await service.ExecuteAsync(cancellationToken);

        await host.StopAsync(cancellationToken);

        return exitCode;
    }
}
