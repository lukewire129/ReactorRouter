namespace ReactorRouter.Routing;

/// <summary>
/// Defines a single route in the routing tree.
/// </summary>
public sealed class RouteDefinition
{
    public RouteDefinition(string path, Type componentType, params RouteDefinition[] children)
    {
        Path = path;
        ComponentType = componentType;
        Children = children;
    }

    /// <summary>Route path segment, e.g. "dashboard", ":userId", "*".</summary>
    public string Path { get; }

    /// <summary>MauiReactor Component type to render when this route is matched.</summary>
    public Type ComponentType { get; }

    /// <summary>Child routes nested under this route.</summary>
    public IReadOnlyList<RouteDefinition> Children { get; }

    /// <summary>Animation to apply when navigating TO this route.</summary>
    public TransitionType? Transition { get; init; }

    /// <summary>Animation duration override in milliseconds.</summary>
    public int? TransitionDuration { get; init; }

    /// <summary>If true, this route matches when parent path is exact (no child segment).</summary>
    public bool IsIndex { get; init; }

    /// <summary>
    /// Synchronous navigation guard. Return <see cref="GuardResult.Allow"/> to permit,
    /// <see cref="GuardResult.Redirect"/> to redirect, or <see cref="GuardResult.Block"/> to cancel.
    /// Evaluated before <see cref="AsyncGuard"/> when both are set.
    /// </summary>
    public Func<RouteGuardContext, GuardResult>? Guard { get; init; }

    /// <summary>
    /// Asynchronous navigation guard. Evaluated when <see cref="Guard"/> is null.
    /// Use for guards that require async operations (e.g. API calls, database checks).
    /// </summary>
    public Func<RouteGuardContext, Task<GuardResult>>? AsyncGuard { get; init; }

    /// <summary>Creates an index route (matches parent path exactly).</summary>
    public static RouteDefinition Index(Type componentType) =>
        new(string.Empty, componentType) { IsIndex = true };

    public RouteDefinition WithTransition(TransitionType transition, int? duration = null) =>
        new(Path, ComponentType, Children.ToArray())
        {
            IsIndex = IsIndex,
            Guard = Guard,
            AsyncGuard = AsyncGuard,
            Transition = transition,
            TransitionDuration = duration
        };
}
