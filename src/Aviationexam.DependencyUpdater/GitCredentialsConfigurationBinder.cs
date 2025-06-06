using Aviationexam.DependencyUpdater.Interfaces;
using System.CommandLine;
using System.CommandLine.Binding;

namespace Aviationexam.DependencyUpdater;

public sealed class GitCredentialsConfigurationBinder(
    Option<string> usernameArgument,
    Option<string> passwordArgument
) : BinderBase<GitCredentialsConfiguration>
{
    protected override GitCredentialsConfiguration GetBoundValue(BindingContext bindingContext) => new()
    {
        Username = bindingContext.ParseResult.GetValueForOption(usernameArgument)!,
        Password = bindingContext.ParseResult.GetValueForOption(passwordArgument)!,
    };
}
