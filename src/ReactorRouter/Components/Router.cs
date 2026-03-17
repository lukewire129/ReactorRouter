using MauiReactor.Parameters;
namespace ReactorRouter.Components;

/// <summary>
/// Top-level routing component. Manages navigation state and provides RouterContext
/// to all descendant components via [Param] IParameter&lt;RouterContext&gt;.
///
/// Usage:
/// <code>
/// new Router()
///     .Routes(routes)
///     .InitialPath("/dashboard")
/// </code>
/// </summary>
public partial class Router : Component<RouterState>
{
    [Prop] private IReadOnlyList<RouteDefinition> _routes = [];
    [Prop] private string _initialPath = "/";

    [Param] IParameter<RouterContext> _routerContext;

    protected override void OnMounted()
    {
        // UseReactorRouter() config takes priority; fall back to [Prop] for backwards compat
        var config = ReactorRouterConfig.Current;
        var routes = config?.RouteDefinitions ?? _routes;
        var initialPath = config?.InitialRoute ?? _initialPath;

        NavigationService.Instance.Initialize(routes);
        NavigationService.Instance.ContextChanged += OnContextChanged;

        NavigationService.Instance.NavigateInitial(initialPath);
    }

    protected override void OnWillUnmount()
    {
        NavigationService.Instance.ContextChanged -= OnContextChanged;
    }

    private void OnContextChanged(RouterContext context)
    {
        SetState(s => s.Context = context);
        // IParameter<T>.Set(Action<T>) mutates the shared instance in-place
        _routerContext.Set(ctx =>
        {
            ctx.MatchChain = context.MatchChain;
            ctx.Params = context.Params;
            ctx.Query = context.Query;
            ctx.CurrentPath = context.CurrentPath;
        });
    }

    public override VisualNode Render()
    {
        var chain = State.Context.MatchChain;

        // 루트 컴포넌트(depth=0) 렌더 — Outlet들은 NavigationService에 등록해 알아서 업데이트됨
        if (chain.Length == 0)
            return ContentView();

        var rootType = chain[0].ComponentType;
        return ComponentFactory.Create(rootType);
    }
}
