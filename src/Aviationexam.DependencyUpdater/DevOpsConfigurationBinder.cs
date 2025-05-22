using Aviationexam.DependencyUpdater.Repository.DevOps;
using System.CommandLine;
using System.CommandLine.Binding;

namespace Aviationexam.DependencyUpdater;

public sealed class DevOpsConfigurationBinder(
    Option<string> organization,
    Option<string> project,
    Option<string> repository,
    Option<string> pat,
    Option<string> accountId
) : BinderBase<DevOpsConfiguration>
{
    protected override DevOpsConfiguration GetBoundValue(BindingContext bindingContext) => new()
    {
        Organization = bindingContext.ParseResult.GetValueForOption(organization)!,
        Project = bindingContext.ParseResult.GetValueForOption(project)!,
        Repository = bindingContext.ParseResult.GetValueForOption(repository)!,
        PersonalAccessToken = bindingContext.ParseResult.GetValueForOption(pat)!,
        AccountId = bindingContext.ParseResult.GetValueForOption(accountId)!,
    };
}
