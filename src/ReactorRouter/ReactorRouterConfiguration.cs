using System.Diagnostics.CodeAnalysis;

namespace ReactorRouter;

/// <summary>
/// Configuration builder for ReactorRouter. Used with UseReactorRouter() in MauiProgram.cs.
/// Automatically registers all component types found in the route tree.
/// </summary>
public class ReactorRouterConfiguration
{
    internal IReadOnlyList<RouteDefinition> RouteDefinitions { get; private set; } = [];
    internal string InitialRoute { get; private set; } = "/";

    /// <summary>
    /// Defines the route tree and automatically registers all component types for AOT-safe creation.
    /// </summary>
    public ReactorRouterConfiguration Routes(params RouteDefinition[] routes)
    {
        RouteDefinitions = routes;
        RegisterComponentTypes(routes);
        return this;
    }

    /// <summary>
    /// Sets the initial navigation path. Defaults to "/".
    /// </summary>
    public ReactorRouterConfiguration InitialPath(string path)
    {
        InitialRoute = path;
        return this;
    }

    [UnconditionalSuppressMessage("Trimming", "IL2067",
        Justification = "Component types in RouteDefinition are explicitly provided by the user and will be preserved.")]
    private static void RegisterComponentTypes(IEnumerable<RouteDefinition> routes)
    {
        foreach (var route in routes)
        {
            var type = route.ComponentType;
            if (ComponentRegistry.GetFactory(type) == null)
            {
                // Capture type in a local to avoid closure issues in the lambda
                var capturedType = type;
                ComponentRegistry.Register(capturedType,
                    () => (VisualNode)Activator.CreateInstance(capturedType)!);
            }

            RegisterComponentTypes(route.Children);
        }
    }
}
