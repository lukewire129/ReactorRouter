using System.Collections.Concurrent;

namespace ReactorRouter.Navigation;

/// <summary>
/// Singleton navigation service. Manages history stack, route matching,
/// and dispatches route updates to registered Outlet instances.
/// </summary>
public sealed class NavigationService
{
    private static readonly Lazy<NavigationService> _instance =
        new(() => new NavigationService());

    public static NavigationService Instance => _instance.Value;

    private NavigationService() { }

    // Route definitions — set once by Router on mount
    private IReadOnlyList<RouteDefinition> _routes = [];

    // Navigation history stack
    private readonly Stack<string> _history = new();

    // Registered outlets by depth (depth → outlet callback)
    private readonly ConcurrentDictionary<int, OutletRegistration> _outlets = new();

    // Current navigation state — used to immediately hydrate late-mounting Outlets
    private RouteMatch[] _currentMatchChain = [];
    private RouteParams _currentMergedParams = RouteParams.Empty;
    private RouteQuery _currentQuery = RouteQuery.Empty;

    // Router subscribes to get context updates → triggers SetState
    public event Action<RouterContext>? ContextChanged;

    public string CurrentPath { get; private set; } = "/";
    public bool CanGoBack => _history.Count > 1;

    /// <summary>Called by Router on mount to supply the route definitions.</summary>
    internal void Initialize(IReadOnlyList<RouteDefinition> routes)
    {
        _routes = routes;
    }

    /// <summary>Called by Outlet when it mounts — returns the assigned depth.</summary>
    internal int RegisterOutlet(OutletRegistration registration)
    {
        // Depth is assigned sequentially: find first unused depth
        int depth = 0;
        while (!_outlets.TryAdd(depth, registration))
            depth++;

        registration.Depth = depth;

        // If navigation already happened before this Outlet mounted (initial render),
        // immediately hydrate it with the current state so it doesn't start blank.
        if (depth < _currentMatchChain.Length)
        {
            var match = _currentMatchChain[depth];
            registration.UpdateRoute(match.ComponentType, _currentMergedParams, _currentQuery);
        }

        return depth;
    }

    internal void UnregisterOutlet(int depth) =>
        _outlets.TryRemove(depth, out _);

    /// <summary>Navigate to the given path (with optional query string).</summary>
    public void NavigateTo(string path)
    {
        if (path == CurrentPath) return;
        _history.Push(path);
        DispatchNavigation(path);
    }

    internal void NavigateInitial(string path)
    {
        _history.Push(path);
        DispatchNavigation(path);
    }

    /// <summary>Navigate back in history.</summary>
    public void GoBack()
    {
        if (!CanGoBack) return;
        _history.Pop(); // discard current
        var previous = _history.Peek();
        DispatchNavigation(previous);
    }

    private void DispatchNavigation(string path)
    {
        CurrentPath = path;
        var (_, query) = RouteParser.Parse(path);
        var matchChain = RouteMatcher.Match(path, _routes) ?? [];

        // Merge all params from the chain
        var mergedParams = matchChain.Length > 0
            ? matchChain[^1].Params
            : RouteParams.Empty;

        // Cache current state so late-mounting Outlets can hydrate themselves
        _currentMatchChain = matchChain;
        _currentMergedParams = mergedParams;
        _currentQuery = query;

        var context = new RouterContext
        {
            CurrentPath = path,
            MatchChain = matchChain,
            Params = mergedParams,
            Query = query
        };

        // Notify Router (triggers its SetState)
        ContextChanged?.Invoke(context);

        // Dispatch to each registered outlet
        for (int depth = 0; depth < matchChain.Length; depth++)
        {
            if (_outlets.TryGetValue(depth, out var outlet))
            {
                var match = matchChain[depth];
                outlet.UpdateRoute(match.ComponentType, mergedParams, query);
            }
        }

        // Clear outlets deeper than the new match chain
        foreach (var key in _outlets.Keys.Where(k => k >= matchChain.Length))
        {
            if (_outlets.TryGetValue(key, out var outlet))
                outlet.UpdateRoute(null, RouteParams.Empty, RouteQuery.Empty);
        }
    }
}

/// <summary>Registration entry for an Outlet at a given depth.</summary>
public sealed class OutletRegistration
{
    public int Depth { get; internal set; }
    internal Action<Type?, RouteParams, RouteQuery> UpdateRouteCallback { get; }

    public OutletRegistration(Action<Type?, RouteParams, RouteQuery> updateCallback)
    {
        UpdateRouteCallback = updateCallback;
    }

    internal void UpdateRoute(Type? componentType, RouteParams @params, RouteQuery query) =>
        UpdateRouteCallback(componentType, @params, query);
}
