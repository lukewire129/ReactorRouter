using System.Collections.Concurrent;

namespace ReactorRouter.Navigation;

/// <summary>
/// Singleton façade for all router instances.
/// Maintains a registry of named <see cref="RouterInstance"/>s and exposes the public
/// navigation API. Methods without a <c>routerName</c> parameter target the "default" router,
/// preserving full backwards compatibility.
/// </summary>
public sealed class NavigationService
{
    private static readonly Lazy<NavigationService> _instance =
        new(() => new NavigationService());

    public static NavigationService Instance => _instance.Value;

    private NavigationService() { }

    private readonly ConcurrentDictionary<string, RouterInstance> _routers = new();

    // ── Navigation events ──────────────────────────────────────────────────

    /// <summary>
    /// Raised after guard evaluation, before the route transition.
    /// Set <see cref="BeforeNavigationEventArgs.Cancel"/> to true to abort.
    /// </summary>
    public event EventHandler<BeforeNavigationEventArgs>? OnBeforeNavigate;

    /// <summary>Raised after the route transition completes.</summary>
    public event EventHandler<NavigationEventArgs>? OnAfterNavigate;

    internal void RaiseBeforeNavigate(BeforeNavigationEventArgs args) =>
        OnBeforeNavigate?.Invoke(this, args);

    internal void RaiseAfterNavigate(NavigationEventArgs args) =>
        OnAfterNavigate?.Invoke(this, args);

    // ── Registry ───────────────────────────────────────────────────────────

    internal void Register(RouterInstance instance) =>
        _routers[instance.Name] = instance;

    internal void Unregister(string name) =>
        _routers.TryRemove(name, out _);

    /// <summary>Returns true if a router with the given name is currently mounted.</summary>
    public bool HasRouter(string routerName) => _routers.ContainsKey(routerName);

    // ── Outlet registration (called by Outlet component) ──────────────────

    internal int RegisterOutlet(string routerName, OutletRegistration registration) =>
        GetRouter(routerName).RegisterOutlet(registration);

    internal void UnregisterOutlet(string routerName, int depth) =>
        GetRouter(routerName).UnregisterOutlet(depth);

    // ── Public API — default router (backwards compatible) ─────────────────

    /// <summary>Current path of the default router.</summary>
    public string CurrentPath => Default.CurrentPath;

    /// <summary>True when the default router can navigate back.</summary>
    public bool CanGoBack => Default.CanGoBack;

    /// <summary>Current merged route params of the default router.</summary>
    public RouteParams CurrentParams => Default.CurrentParams;

    /// <summary>Current query params of the default router.</summary>
    public RouteQuery CurrentQuery => Default.CurrentQuery;

    /// <summary>Navigate the default router to <paramref name="path"/>.</summary>
    public void NavigateTo(string path) => Default.NavigateTo(path);

    /// <summary>Navigate the default router to <paramref name="path"/>, optionally force-reloading.</summary>
    public void NavigateTo(string path, bool forceReload) => Default.NavigateTo(path, forceReload);

    /// <summary>Navigate the default router back one step.</summary>
    public void GoBack() => Default.GoBack();

    /// <summary>Reload the current route on the default router.</summary>
    public void Reload() => Default.Reload();

    /// <summary>Reload the current route on the default router, optionally skipping guard evaluation.</summary>
    public void Reload(bool skipGuard) => Default.Reload(skipGuard);

    // ── Public API — named router ──────────────────────────────────────────

    /// <summary>Navigate a named router to <paramref name="path"/>.</summary>
    public void NavigateTo(string routerName, string path) =>
        GetRouter(routerName).NavigateTo(path);

    /// <summary>Navigate a named router to <paramref name="path"/>, optionally force-reloading.</summary>
    public void NavigateTo(string routerName, string path, bool forceReload) =>
        GetRouter(routerName).NavigateTo(path, forceReload);

    /// <summary>Navigate a named router back one step.</summary>
    public void GoBack(string routerName) => GetRouter(routerName).GoBack();

    /// <summary>Reload the current route on a named router.</summary>
    public void Reload(string routerName) => GetRouter(routerName).Reload();

    /// <summary>Reload the current route on a named router, optionally skipping guard evaluation.</summary>
    public void Reload(string routerName, bool skipGuard) => GetRouter(routerName).Reload(skipGuard);

    // ── Queries ────────────────────────────────────────────────────────────

    /// <summary>Returns the current path of the specified router (defaults to "default").</summary>
    public string GetCurrentPath(string routerName = "default") =>
        GetRouter(routerName).CurrentPath;

    // ── Helpers ────────────────────────────────────────────────────────────

    private RouterInstance Default =>
        _routers.TryGetValue("default", out var r)
            ? r
            : throw new InvalidOperationException(
                "No default router is mounted. Ensure a Router component is in the component tree.");

    private RouterInstance GetRouter(string name) =>
        _routers.TryGetValue(name, out var r)
            ? r
            : throw new InvalidOperationException(
                $"Router '{name}' not found. Ensure the Router component with Name(\"{name}\") is mounted.");

    // ── Internal initial navigation (called by Router on mount) ───────────

    internal void NavigateInitial(string routerName, string path) =>
        GetRouter(routerName).NavigateInitial(path);
}

/// <summary>Registration entry for an Outlet at a given depth within a router.</summary>
public sealed class OutletRegistration
{
    public int Depth { get; internal set; }
    internal Action<Type?, RouteParams, RouteQuery, bool> UpdateRouteCallback { get; }

    public OutletRegistration(Action<Type?, RouteParams, RouteQuery, bool> updateCallback)
    {
        UpdateRouteCallback = updateCallback;
    }

    internal void UpdateRoute(
        Type? componentType,
        RouteParams @params,
        RouteQuery query,
        bool forceRemount = false) =>
        UpdateRouteCallback(componentType, @params, query, forceRemount);
}
