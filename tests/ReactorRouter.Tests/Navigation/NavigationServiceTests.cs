namespace ReactorRouter.Tests.Navigation;

// Minimal component stubs
file class PageA;
file class PageB;
file class PageC;

/// <summary>
/// Tests for NavigationService named router registry and multi-router API.
/// </summary>
[Collection("NavigationService")]
public class NavigationServiceNamedRouterTests : IDisposable
{
    private static readonly IReadOnlyList<RouteDefinition> SimpleRoutes =
    [
        new RouteDefinition("/", typeof(PageA),
            new RouteDefinition("page-b", typeof(PageB)),
            new RouteDefinition("page-c", typeof(PageC))
        )
    ];

    private readonly List<string> _registeredNames = [];

    private RouterInstance CreateAndRegister(string name = "default")
    {
        var instance = new RouterInstance(name);
        instance.Initialize(SimpleRoutes);
        NavigationService.Instance.Register(instance);
        _registeredNames.Add(name);
        return instance;
    }

    public void Dispose()
    {
        foreach (var name in _registeredNames)
            NavigationService.Instance.Unregister(name);
        _registeredNames.Clear();
    }

    // ── HasRouter ──────────────────────────────────────────────────────────

    [Fact]
    public void HasRouter_ReturnsFalse_WhenNotRegistered()
    {
        NavigationService.Instance.HasRouter("nonexistent").Should().BeFalse();
    }

    [Fact]
    public void HasRouter_ReturnsTrue_AfterRegister()
    {
        CreateAndRegister("my-router");
        NavigationService.Instance.HasRouter("my-router").Should().BeTrue();
    }

    [Fact]
    public void HasRouter_ReturnsFalse_AfterUnregister()
    {
        CreateAndRegister("temp-router");
        NavigationService.Instance.Unregister("temp-router");
        _registeredNames.Remove("temp-router");

        NavigationService.Instance.HasRouter("temp-router").Should().BeFalse();
    }

    // ── Default router API (backwards compat) ──────────────────────────────

    [Fact]
    public async Task NavigateTo_DefaultRouter_UpdatesCurrentPath()
    {
        var instance = CreateAndRegister("default");
        instance.NavigateInitial("/");

        NavigationService.Instance.NavigateTo("/page-b");
        await instance.LastNavigationTask;

        NavigationService.Instance.CurrentPath.Should().Be("/page-b");
    }

    [Fact]
    public async Task GoBack_DefaultRouter_NavigatesBack()
    {
        var instance = CreateAndRegister("default");
        instance.NavigateInitial("/");

        NavigationService.Instance.NavigateTo("/page-b");
        await instance.LastNavigationTask;
        NavigationService.Instance.GoBack();
        await instance.LastNavigationTask;

        NavigationService.Instance.CurrentPath.Should().Be("/");
    }

    [Fact]
    public async Task CanGoBack_DefaultRouter_TrueAfterTwoNavigations()
    {
        var instance = CreateAndRegister("default");
        instance.NavigateInitial("/");

        NavigationService.Instance.NavigateTo("/page-b");
        await instance.LastNavigationTask;

        NavigationService.Instance.CanGoBack.Should().BeTrue();
    }

    [Fact]
    public async Task GetCurrentPath_DefaultRouter()
    {
        var instance = CreateAndRegister("default");
        instance.NavigateInitial("/");

        NavigationService.Instance.NavigateTo("/page-c");
        await instance.LastNavigationTask;

        NavigationService.Instance.GetCurrentPath().Should().Be("/page-c");
        NavigationService.Instance.GetCurrentPath("default").Should().Be("/page-c");
    }

    // ── Named router API ───────────────────────────────────────────────────

    [Fact]
    public async Task NavigateTo_NamedRouter_UpdatesNamedRouterPath()
    {
        var main = CreateAndRegister("main");
        main.NavigateInitial("/");

        NavigationService.Instance.NavigateTo("main", "/page-b");
        await main.LastNavigationTask;

        NavigationService.Instance.GetCurrentPath("main").Should().Be("/page-b");
    }

    [Fact]
    public async Task NavigateTo_NamedRouter_DoesNotAffectOtherRouter()
    {
        var main = CreateAndRegister("main");
        var detail = CreateAndRegister("detail");
        main.NavigateInitial("/");
        detail.NavigateInitial("/");

        NavigationService.Instance.NavigateTo("main", "/page-b");
        await main.LastNavigationTask;

        NavigationService.Instance.GetCurrentPath("detail").Should().Be("/");
    }

    [Fact]
    public async Task TwoNamedRouters_NavigateIndependently()
    {
        var main = CreateAndRegister("main");
        var detail = CreateAndRegister("detail");
        main.NavigateInitial("/");
        detail.NavigateInitial("/");

        NavigationService.Instance.NavigateTo("main", "/page-b");
        await main.LastNavigationTask;
        NavigationService.Instance.NavigateTo("detail", "/page-c");
        await detail.LastNavigationTask;

        NavigationService.Instance.GetCurrentPath("main").Should().Be("/page-b");
        NavigationService.Instance.GetCurrentPath("detail").Should().Be("/page-c");
    }

    [Fact]
    public async Task GoBack_NamedRouter_NavigatesBack()
    {
        var detail = CreateAndRegister("detail");
        detail.NavigateInitial("/");

        NavigationService.Instance.NavigateTo("detail", "/page-b");
        await detail.LastNavigationTask;
        NavigationService.Instance.GoBack("detail");
        await detail.LastNavigationTask;

        NavigationService.Instance.GetCurrentPath("detail").Should().Be("/");
    }

    [Fact]
    public async Task Reload_NamedRouter_StaysOnCurrentPath()
    {
        var detail = CreateAndRegister("detail");
        detail.NavigateInitial("/");

        NavigationService.Instance.NavigateTo("detail", "/page-b");
        await detail.LastNavigationTask;

        NavigationService.Instance.Reload("detail");
        await detail.LastNavigationTask;

        NavigationService.Instance.GetCurrentPath("detail").Should().Be("/page-b");
    }

    [Fact]
    public async Task NavigateTo_ForceReload_NamedRouter_TriggersNewNavigationEvent()
    {
        var detail = CreateAndRegister("detail");
        detail.NavigateInitial("/");
        await detail.LastNavigationTask; // settle before subscribing

        int afterCount = 0;
        EventHandler<NavigationEventArgs> handler = (_, args) =>
        {
            if (args.RouterName == "detail") afterCount++;
        };
        NavigationService.Instance.OnAfterNavigate += handler;

        try
        {
            NavigationService.Instance.NavigateTo("detail", "/page-b");
            await detail.LastNavigationTask;

            NavigationService.Instance.NavigateTo("detail", "/page-b", forceReload: true);
            await detail.LastNavigationTask;

            afterCount.Should().Be(2); // once for navigate, once for force-reload
            NavigationService.Instance.GetCurrentPath("detail").Should().Be("/page-b");
        }
        finally
        {
            NavigationService.Instance.OnAfterNavigate -= handler;
        }
    }

    // ── Error cases ────────────────────────────────────────────────────────

    [Fact]
    public void NavigateTo_UnknownRouter_ThrowsInvalidOperationException()
    {
        var act = () => NavigationService.Instance.NavigateTo("ghost", "/page-b");
        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*ghost*");
    }

    [Fact]
    public void GoBack_UnknownRouter_ThrowsInvalidOperationException()
    {
        var act = () => NavigationService.Instance.GoBack("ghost");
        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void GetCurrentPath_UnknownRouter_ThrowsInvalidOperationException()
    {
        var act = () => NavigationService.Instance.GetCurrentPath("ghost");
        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void NavigateTo_NoDefaultRouter_ThrowsInvalidOperationException()
    {
        // Ensure "default" is not registered
        NavigationService.Instance.Unregister("default");
        _registeredNames.Remove("default");

        var act = () => NavigationService.Instance.NavigateTo("/anywhere");
        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*default router*");
    }
}
