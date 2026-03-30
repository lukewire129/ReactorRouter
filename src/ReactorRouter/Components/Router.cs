using MauiReactor.Parameters;
namespace ReactorRouter.Components;

/// <summary>
/// Top-level routing component. Manages navigation state and provides RouterContext
/// to all descendant components via [Param] IParameter&lt;RouterContext&gt;.
///
/// Usage (single / default router):
/// <code>
/// new Router()
///     .Routes(routes)
///     .InitialPath("/dashboard")
/// </code>
///
/// Usage (named router for multi-router layouts):
/// <code>
/// new Router()
///     .Name("detail")
///     .Routes(detailRoutes)
///     .InitialPath("/empty")
/// </code>
/// </summary>
public partial class Router : Component<RouterState>
{
    [Prop] private IReadOnlyList<RouteDefinition> _routes = [];
    [Prop] private string _initialPath = "/";
    [Prop] private string _name = "default";

    [Param] IParameter<RouterContext> _routerContext;

    // Holds the per-router instance for the lifetime of this component.
    // Plain field — survives re-renders since the component instance is reused by MauiReactor.
    private RouterInstance? _instance;

    protected override void OnMounted()
    {
        IReadOnlyList<RouteDefinition> routes;
        string initialPath;

        // UseReactorRouter() config applies only to the default router for backwards compat.
        if (_name == "default")
        {
            var config = ReactorRouterConfig.Current;
            routes = config?.RouteDefinitions ?? _routes;
            initialPath = config?.InitialRoute ?? _initialPath;
        }
        else
        {
            routes = _routes;
            initialPath = _initialPath;
        }

        _instance = new RouterInstance(_name);
        _instance.Initialize(routes);
        _instance.ContextChanged += OnContextChanged;

        NavigationService.Instance.Register(_instance);
        _instance.NavigateInitial(initialPath);
    }

    protected override void OnWillUnmount()
    {
        if (_instance != null)
        {
            _instance.ContextChanged -= OnContextChanged;
            NavigationService.Instance.Unregister(_name);
            _instance = null;
        }
    }

    private void OnContextChanged(RouterContext context)
    {
        SetState(s => s.Context = context);
        _routerContext.Set(ctx =>
        {
            ctx.MatchChain = context.MatchChain;
            ctx.Params = context.Params;
            ctx.Query = context.Query;
            ctx.CurrentPath = context.CurrentPath;
            ctx.RouterName = context.RouterName;
        });
    }

    public override VisualNode Render()
    {
        var chain = State.Context.MatchChain;

        if (chain.Length == 0)
            return ContentView();

        return ComponentFactory.Create(chain[0].ComponentType);
    }
}
