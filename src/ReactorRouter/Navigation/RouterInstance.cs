using System.Collections.Concurrent;

namespace ReactorRouter.Navigation;

/// <summary>
/// Encapsulates all per-router state: routes, history, outlets, and the full navigation pipeline.
/// Created by <see cref="Router"/> on mount and registered with <see cref="NavigationService"/>.
/// </summary>
internal sealed class RouterInstance
{
    private IReadOnlyList<RouteDefinition> _routes = [];
    private readonly Stack<string> _history = new();
    private readonly ConcurrentDictionary<int, OutletRegistration> _outlets = new();

    private RouteMatch[] _currentMatchChain = [];
    private RouteParams _currentMergedParams = RouteParams.Empty;
    private RouteQuery _currentQuery = RouteQuery.Empty;

    public string Name { get; }
    public string CurrentPath { get; private set; } = "/";
    public bool CanGoBack => _history.Count > 1;
    public RouteParams CurrentParams => _currentMergedParams;
    public RouteQuery CurrentQuery => _currentQuery;

    /// <summary>
    /// The Task for the most recently started navigation pipeline.
    /// Useful in tests to await pipeline completion before asserting state.
    /// </summary>
    internal Task LastNavigationTask { get; private set; } = Task.CompletedTask;

    /// <summary>Fired after a navigation commits. Router subscribes to trigger SetState.</summary>
    public event Action<RouterContext>? ContextChanged;

    internal RouterInstance(string name) => Name = name;

    internal void Initialize(IReadOnlyList<RouteDefinition> routes) => _routes = routes;

    // ── Outlet registration ────────────────────────────────────────────────

    internal int RegisterOutlet(OutletRegistration registration)
    {
        int depth = 0;
        while (!_outlets.TryAdd(depth, registration))
            depth++;

        registration.Depth = depth;

        // Hydrate late-mounting Outlet with current state
        var chainIndex = depth + 1;
        if (chainIndex < _currentMatchChain.Length)
        {
            var match = _currentMatchChain[chainIndex];
            registration.UpdateRoute(match.ComponentType, _currentMergedParams, _currentQuery);
        }

        return depth;
    }

    internal void UnregisterOutlet(int depth) => _outlets.TryRemove(depth, out _);

    // ── Navigation API ─────────────────────────────────────────────────────

    internal void NavigateInitial(string path)
    {
        _history.Push(path);
        LastNavigationTask = DispatchNavigationAsync(path);
    }

    /// <summary>Navigate to <paramref name="path"/>. No-op if already there (unless forceReload).</summary>
    public void NavigateTo(string path, bool forceReload = false)
    {
        bool isSamePath = path == CurrentPath;
        if (isSamePath && !forceReload) return;

        if (!isSamePath)
            _history.Push(path);

        LastNavigationTask = DispatchNavigationAsync(path, isReload: isSamePath && forceReload);
    }

    /// <summary>Navigate back one step in history.</summary>
    public void GoBack()
    {
        if (!CanGoBack) return;
        _history.Pop();
        var previous = _history.Peek();
        LastNavigationTask = DispatchNavigationAsync(previous);
    }

    /// <summary>Unmount and remount the current route's component.</summary>
    public void Reload(bool skipGuard = false)
    {
        LastNavigationTask = DispatchNavigationAsync(CurrentPath, isReload: true, skipGuard: skipGuard);
    }

    // ── Core pipeline ──────────────────────────────────────────────────────

    private const int MaxGuardRedirectDepth = 10;

    private async Task DispatchNavigationAsync(
        string path,
        bool isReload = false,
        bool skipGuard = false,
        int guardDepth = 0)
    {
        if (guardDepth > MaxGuardRedirectDepth) return;

        var (_, query) = RouteParser.Parse(path);
        var matchChain = RouteMatcher.Match(path, _routes) ?? [];
        var mergedParams = matchChain.Length > 0 ? matchChain[^1].Params : RouteParams.Empty;

        // 1. Guard evaluation (top-down, parent before child)
        if (!skipGuard)
        {
            foreach (var match in matchChain)
            {
                GuardResult guardResult = new GuardResult.Allow();
                var ctx = new RouteGuardContext
                {
                    FromPath = CurrentPath,
                    ToPath = path,
                    Params = mergedParams,
                    QueryParams = query
                };

                if (match.Route.Guard != null)
                    guardResult = match.Route.Guard(ctx);
                else if (match.Route.AsyncGuard != null)
                    guardResult = await match.Route.AsyncGuard(ctx).ConfigureAwait(false);

                if (guardResult is GuardResult.Block) return;

                if (guardResult is GuardResult.Redirect redirect)
                {
                    // Push redirect target to history only if it differs from current
                    if (redirect.Path != CurrentPath)
                        _history.Push(redirect.Path);

                    await DispatchNavigationAsync(
                        redirect.Path,
                        isReload: false,
                        guardDepth: guardDepth + 1).ConfigureAwait(false);
                    return;
                }
            }
        }

        // 2. OnBeforeNavigate event
        var beforeArgs = new BeforeNavigationEventArgs
        {
            FromPath = CurrentPath,
            ToPath = path,
            IsReload = isReload,
            RouterName = Name
        };
        NavigationService.Instance.RaiseBeforeNavigate(beforeArgs);
        if (beforeArgs.Cancel) return;

        // 3. Commit navigation state
        var fromPath = CurrentPath;
        CurrentPath = path;
        _currentMatchChain = matchChain;
        _currentMergedParams = mergedParams;
        _currentQuery = query;

        var context = new RouterContext
        {
            CurrentPath = path,
            MatchChain = matchChain,
            Params = mergedParams,
            Query = query,
            RouterName = Name
        };

        // Notify Router component (triggers its SetState)
        ContextChanged?.Invoke(context);

        // 4. Dispatch to registered Outlets
        var outletCount = matchChain.Length - 1;
        for (int depth = 0; depth < outletCount; depth++)
        {
            if (_outlets.TryGetValue(depth, out var outlet))
            {
                var match = matchChain[depth + 1];
                outlet.UpdateRoute(match.ComponentType, mergedParams, query, forceRemount: isReload);
            }
        }

        // Clear outlets deeper than the new match chain
        foreach (var key in _outlets.Keys.Where(k => k >= outletCount))
        {
            if (_outlets.TryGetValue(key, out var outlet))
                outlet.UpdateRoute(null, RouteParams.Empty, RouteQuery.Empty);
        }

        // 5. OnAfterNavigate event
        NavigationService.Instance.RaiseAfterNavigate(new NavigationEventArgs
        {
            FromPath = fromPath,
            ToPath = path,
            IsReload = isReload,
            RouterName = Name
        });
    }
}
