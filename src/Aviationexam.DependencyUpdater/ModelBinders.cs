using System;
using System.Collections.Generic;
using System.CommandLine.Binding;
using System.CommandLine.Invocation;

namespace Aviationexam.DependencyUpdater;

public sealed class ModelBinders
{
    private readonly Dictionary<(Type Model, Type ModelBinder), Func<InvocationContext, object, object>> _binders = new();

    public IReadOnlyDictionary<(Type Model, Type ModelBinder), Func<InvocationContext, object, object>> Binders => _binders;

    public void AddModelBinder<TModel, TBinder>(
        Func<InvocationContext, TBinder, TModel> binder
    ) where TBinder : BinderBase<TModel>
    {
        _binders.Add((typeof(TModel), typeof(TBinder)), (invocationContext, modelBinder) => binder(invocationContext, (TBinder) modelBinder)!);
    }
}
