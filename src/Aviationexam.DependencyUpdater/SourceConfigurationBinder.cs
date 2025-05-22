using Aviationexam.DependencyUpdater.Common;
using System.CommandLine;
using System.CommandLine.Binding;

namespace Aviationexam.DependencyUpdater;

public sealed class SourceConfigurationBinder(
    Option<string> directoryArgument
) : BinderBase<SourceConfiguration>
{
    protected override SourceConfiguration GetBoundValue(BindingContext bindingContext) => new()
    {
        Directory = bindingContext.ParseResult.GetValueForOption(directoryArgument)!,
    };
}
