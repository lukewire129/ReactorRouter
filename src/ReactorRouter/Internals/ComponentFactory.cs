using System.Runtime.CompilerServices;

namespace ReactorRouter.Internals;

/// <summary>
/// Shared component creation logic. Checks ComponentRegistry first (AOT-safe),
/// falls back to Activator.CreateInstance for JIT/Hot Reload environments.
/// </summary>
internal static class ComponentFactory
{
    internal static VisualNode Create(Type componentType)
    {
        // AOT-safe path: use registered factory
        var factory = ComponentRegistry.GetFactory(componentType);
        if (factory != null)
            return factory();

        // JIT/Hot Reload fallback
        if (!RuntimeFeature.IsDynamicCodeSupported)
            throw new InvalidOperationException(
                $"Component '{componentType.FullName}' is not registered in ComponentRegistry. " +
                $"In AOT mode, all routed components must be registered. " +
                $"Use UseReactorRouter() in MauiProgram.cs to define routes and auto-register components.");

        return (VisualNode)Activator.CreateInstance(TypeResolver.ResolveLatest(componentType))!;
    }
}
