namespace ReactorRouter.Tests.Navigation;

// Dummy component types for routing
file class RootLayout;
file class DashboardPage;
file class LoginPage;
file class AdminPage;
file class SettingsPage;
file class PremiumPage;

/// <summary>
/// Tests for RouterInstance navigation pipeline:
/// - Basic navigation, GoBack
/// - Sync and async guard evaluation (Allow / Block / Redirect)
/// - Reload
/// - OnBeforeNavigate / OnAfterNavigate events
/// </summary>
[Collection("NavigationService")]
public class RouterInstanceTests : IDisposable
{
    // Simple route tree used across most tests
    private static readonly IReadOnlyList<RouteDefinition> Routes =
    [
        new RouteDefinition("/", typeof(RootLayout),
            new RouteDefinition("dashboard", typeof(DashboardPage)),
            new RouteDefinition("login", typeof(LoginPage)),
            new RouteDefinition("admin", typeof(AdminPage),
                new RouteDefinition("settings", typeof(SettingsPage))
            ),
            new RouteDefinition("premium", typeof(PremiumPage))
        )
    ];

    private RouterInstance CreateInstance(string name = "test", IReadOnlyList<RouteDefinition>? routes = null)
    {
        var instance = new RouterInstance(name);
        instance.Initialize(routes ?? Routes);
        return instance;
    }

    // Clean up NavigationService event subscriptions between tests
    public void Dispose()
    {
        NavigationService.Instance.OnBeforeNavigate -= _beforeHandler;
        NavigationService.Instance.OnAfterNavigate -= _afterHandler;
    }

    private EventHandler<BeforeNavigationEventArgs>? _beforeHandler;
    private EventHandler<NavigationEventArgs>? _afterHandler;

    // ── Basic navigation ───────────────────────────────────────────────────

    [Fact]
    public async Task NavigateTo_UpdatesCurrentPath()
    {
        var instance = CreateInstance();
        instance.NavigateTo("/dashboard");
        await instance.LastNavigationTask;

        instance.CurrentPath.Should().Be("/dashboard");
    }

    [Fact]
    public async Task NavigateTo_SamePath_IsNoOp_CurrentPathUnchanged()
    {
        var instance = CreateInstance();
        instance.NavigateTo("/dashboard");
        await instance.LastNavigationTask;

        var taskBefore = instance.LastNavigationTask;
        instance.NavigateTo("/dashboard"); // same path, no forceReload

        // LastNavigationTask should not have changed (no new navigation started)
        instance.LastNavigationTask.Should().BeSameAs(taskBefore);
        instance.CurrentPath.Should().Be("/dashboard");
    }

    [Fact]
    public async Task GoBack_ReturnsToPreviousPath()
    {
        var instance = CreateInstance();
        instance.NavigateTo("/login");
        await instance.LastNavigationTask;
        instance.NavigateTo("/dashboard");
        await instance.LastNavigationTask;

        instance.GoBack();
        await instance.LastNavigationTask;

        instance.CurrentPath.Should().Be("/login");
    }

    [Fact]
    public async Task CanGoBack_FalseAfterSingleNavigation()
    {
        var instance = CreateInstance();
        instance.NavigateTo("/login");
        await instance.LastNavigationTask;

        instance.CanGoBack.Should().BeFalse();
    }

    [Fact]
    public async Task CanGoBack_TrueAfterTwoNavigations()
    {
        var instance = CreateInstance();
        instance.NavigateTo("/login");
        await instance.LastNavigationTask;
        instance.NavigateTo("/dashboard");
        await instance.LastNavigationTask;

        instance.CanGoBack.Should().BeTrue();
    }

    // ── Sync guard ─────────────────────────────────────────────────────────

    [Fact]
    public async Task Guard_Allow_PermitsNavigation()
    {
        var routes = new[]
        {
            new RouteDefinition("/", typeof(RootLayout),
                new RouteDefinition("dashboard", typeof(DashboardPage))
                {
                    Guard = _ => new GuardResult.Allow()
                }
            )
        };
        var instance = CreateInstance(routes: routes);
        instance.NavigateTo("/dashboard");
        await instance.LastNavigationTask;

        instance.CurrentPath.Should().Be("/dashboard");
    }

    [Fact]
    public async Task Guard_Block_CancelsNavigation()
    {
        var routes = new[]
        {
            new RouteDefinition("/", typeof(RootLayout),
                new RouteDefinition("dashboard", typeof(DashboardPage))
                {
                    Guard = _ => new GuardResult.Block()
                }
            )
        };
        var instance = CreateInstance(routes: routes);
        instance.NavigateTo("/login"); // establish starting point
        await instance.LastNavigationTask;

        instance.NavigateTo("/dashboard");
        await instance.LastNavigationTask;

        instance.CurrentPath.Should().Be("/login"); // blocked, stayed on /login
    }

    [Fact]
    public async Task Guard_Redirect_NavigatesToTarget()
    {
        var routes = new[]
        {
            new RouteDefinition("/", typeof(RootLayout),
                new RouteDefinition("dashboard", typeof(DashboardPage))
                {
                    Guard = _ => new GuardResult.Redirect("/login")
                },
                new RouteDefinition("login", typeof(LoginPage))
            )
        };
        var instance = CreateInstance(routes: routes);
        instance.NavigateTo("/dashboard");
        await instance.LastNavigationTask;

        instance.CurrentPath.Should().Be("/login");
    }

    [Fact]
    public async Task Guard_ReceivesCorrectContext_FromPath_ToPath()
    {
        RouteGuardContext? capturedCtx = null;
        var routes = new[]
        {
            new RouteDefinition("/", typeof(RootLayout),
                new RouteDefinition("login", typeof(LoginPage)),
                new RouteDefinition("dashboard", typeof(DashboardPage))
                {
                    Guard = ctx =>
                    {
                        capturedCtx = ctx;
                        return new GuardResult.Allow();
                    }
                }
            )
        };
        var instance = CreateInstance(routes: routes);
        instance.NavigateTo("/login");
        await instance.LastNavigationTask;
        instance.NavigateTo("/dashboard");
        await instance.LastNavigationTask;

        capturedCtx.Should().NotBeNull();
        capturedCtx!.FromPath.Should().Be("/login");
        capturedCtx.ToPath.Should().Be("/dashboard");
    }

    [Fact]
    public async Task Guard_ParentBlock_StopsBeforeChildGuardEvaluated()
    {
        bool childGuardEvaluated = false;
        var routes = new[]
        {
            new RouteDefinition("/", typeof(RootLayout),
                new RouteDefinition("admin", typeof(AdminPage),
                    new RouteDefinition("settings", typeof(SettingsPage))
                    {
                        Guard = _ =>
                        {
                            childGuardEvaluated = true;
                            return new GuardResult.Allow();
                        }
                    }
                )
                {
                    Guard = _ => new GuardResult.Block() // parent blocks
                }
            )
        };
        var instance = CreateInstance(routes: routes);
        instance.NavigateTo("/admin/settings");
        await instance.LastNavigationTask;

        childGuardEvaluated.Should().BeFalse();
    }

    [Fact]
    public async Task Guard_Redirect_MaxDepth_DoesNotInfiniteLoop()
    {
        // Guard always redirects to itself → should stop at max depth
        var routes = new[]
        {
            new RouteDefinition("/", typeof(RootLayout),
                new RouteDefinition("loop", typeof(DashboardPage))
                {
                    Guard = _ => new GuardResult.Redirect("/loop")
                }
            )
        };
        var instance = CreateInstance(routes: routes);

        // Should complete without hanging
        var cts = new CancellationTokenSource(TimeSpan.FromSeconds(5));
        var nav = instance.LastNavigationTask;
        instance.NavigateTo("/loop");

        await instance.LastNavigationTask.WaitAsync(cts.Token);

        // Navigation is blocked by max depth — path stays at the pre-redirect value
        instance.CurrentPath.Should().NotBe("/loop");
    }

    // ── Async guard ────────────────────────────────────────────────────────

    [Fact]
    public async Task AsyncGuard_Allow_PermitsNavigation()
    {
        var routes = new[]
        {
            new RouteDefinition("/", typeof(RootLayout),
                new RouteDefinition("premium", typeof(PremiumPage))
                {
                    AsyncGuard = async _ =>
                    {
                        await Task.Yield(); // simulate async work
                        return new GuardResult.Allow();
                    }
                }
            )
        };
        var instance = CreateInstance(routes: routes);
        instance.NavigateTo("/premium");
        await instance.LastNavigationTask;

        instance.CurrentPath.Should().Be("/premium");
    }

    [Fact]
    public async Task AsyncGuard_Block_CancelsNavigation()
    {
        var routes = new[]
        {
            new RouteDefinition("/", typeof(RootLayout),
                new RouteDefinition("login", typeof(LoginPage)),
                new RouteDefinition("premium", typeof(PremiumPage))
                {
                    AsyncGuard = async _ =>
                    {
                        await Task.Yield();
                        return new GuardResult.Block();
                    }
                }
            )
        };
        var instance = CreateInstance(routes: routes);
        instance.NavigateTo("/login");
        await instance.LastNavigationTask;
        instance.NavigateTo("/premium");
        await instance.LastNavigationTask;

        instance.CurrentPath.Should().Be("/login");
    }

    [Fact]
    public async Task AsyncGuard_Redirect_NavigatesToTarget()
    {
        var routes = new[]
        {
            new RouteDefinition("/", typeof(RootLayout),
                new RouteDefinition("premium", typeof(PremiumPage))
                {
                    AsyncGuard = async _ =>
                    {
                        await Task.Yield();
                        return new GuardResult.Redirect("/login");
                    }
                },
                new RouteDefinition("login", typeof(LoginPage))
            )
        };
        var instance = CreateInstance(routes: routes);
        instance.NavigateTo("/premium");
        await instance.LastNavigationTask;

        instance.CurrentPath.Should().Be("/login");
    }

    // ── Reload ─────────────────────────────────────────────────────────────

    [Fact]
    public async Task Reload_StaysOnCurrentPath()
    {
        var instance = CreateInstance();
        instance.NavigateTo("/dashboard");
        await instance.LastNavigationTask;

        instance.Reload();
        await instance.LastNavigationTask;

        instance.CurrentPath.Should().Be("/dashboard");
    }

    [Fact]
    public async Task NavigateTo_ForceReload_SamePath_TriggersNewNavigationEvent()
    {
        int afterCount = 0;
        _afterHandler = (_, _) => afterCount++;
        NavigationService.Instance.OnAfterNavigate += _afterHandler;

        var instance = CreateInstance();
        instance.NavigateTo("/dashboard");
        await instance.LastNavigationTask;

        instance.NavigateTo("/dashboard", forceReload: true);
        await instance.LastNavigationTask;

        // Pipeline ran twice: once for the initial navigate, once for the force-reload
        afterCount.Should().Be(2);
        instance.CurrentPath.Should().Be("/dashboard");
    }

    [Fact]
    public async Task Reload_EvaluatesGuard_ByDefault()
    {
        int guardCallCount = 0;
        var routes = new[]
        {
            new RouteDefinition("/", typeof(RootLayout),
                new RouteDefinition("dashboard", typeof(DashboardPage))
                {
                    Guard = _ =>
                    {
                        guardCallCount++;
                        return new GuardResult.Allow();
                    }
                }
            )
        };
        var instance = CreateInstance(routes: routes);
        instance.NavigateTo("/dashboard");
        await instance.LastNavigationTask;

        instance.Reload();
        await instance.LastNavigationTask;

        guardCallCount.Should().Be(2); // once for navigate, once for reload
    }

    [Fact]
    public async Task Reload_SkipGuard_BypassesGuard()
    {
        int guardCallCount = 0;
        var routes = new[]
        {
            new RouteDefinition("/", typeof(RootLayout),
                new RouteDefinition("dashboard", typeof(DashboardPage))
                {
                    Guard = _ =>
                    {
                        guardCallCount++;
                        return new GuardResult.Allow();
                    }
                }
            )
        };
        var instance = CreateInstance(routes: routes);
        instance.NavigateTo("/dashboard");
        await instance.LastNavigationTask;

        instance.Reload(skipGuard: true);
        await instance.LastNavigationTask;

        guardCallCount.Should().Be(1); // only the initial navigate, reload skipped guard
    }

    // ── Navigation events ─────────────────────────────────────────────────

    [Fact]
    public async Task OnBeforeNavigate_IsFired()
    {
        BeforeNavigationEventArgs? captured = null;
        _beforeHandler = (_, args) => captured = args;
        NavigationService.Instance.OnBeforeNavigate += _beforeHandler;

        var instance = CreateInstance();
        instance.NavigateTo("/dashboard");
        await instance.LastNavigationTask;

        captured.Should().NotBeNull();
    }

    [Fact]
    public async Task OnAfterNavigate_IsFired()
    {
        NavigationEventArgs? captured = null;
        _afterHandler = (_, args) => captured = args;
        NavigationService.Instance.OnAfterNavigate += _afterHandler;

        var instance = CreateInstance();
        instance.NavigateTo("/dashboard");
        await instance.LastNavigationTask;

        captured.Should().NotBeNull();
    }

    [Fact]
    public async Task OnBeforeNavigate_HasCorrectPaths()
    {
        BeforeNavigationEventArgs? captured = null;
        _beforeHandler = (_, args) => captured = args;
        NavigationService.Instance.OnBeforeNavigate += _beforeHandler;

        var instance = CreateInstance();
        instance.NavigateTo("/login");
        await instance.LastNavigationTask;
        instance.NavigateTo("/dashboard");
        await instance.LastNavigationTask;

        captured.Should().NotBeNull();
        captured!.FromPath.Should().Be("/login");
        captured.ToPath.Should().Be("/dashboard");
    }

    [Fact]
    public async Task OnAfterNavigate_HasCorrectPaths()
    {
        NavigationEventArgs? captured = null;
        _afterHandler = (_, args) => captured = args;
        NavigationService.Instance.OnAfterNavigate += _afterHandler;

        var instance = CreateInstance();
        instance.NavigateTo("/login");
        await instance.LastNavigationTask;
        instance.NavigateTo("/dashboard");
        await instance.LastNavigationTask;

        captured!.FromPath.Should().Be("/login");
        captured.ToPath.Should().Be("/dashboard");
    }

    [Fact]
    public async Task OnBeforeNavigate_Cancel_StopsNavigation()
    {
        var instance = CreateInstance();
        // Navigate to /login first, BEFORE subscribing the cancel handler
        instance.NavigateTo("/login");
        await instance.LastNavigationTask;

        // Now subscribe — only subsequent navigations will be cancelled
        _beforeHandler = (_, args) => args.Cancel = true;
        NavigationService.Instance.OnBeforeNavigate += _beforeHandler;

        instance.NavigateTo("/dashboard");
        await instance.LastNavigationTask;

        instance.CurrentPath.Should().Be("/login"); // cancelled, stayed on /login
    }

    [Fact]
    public async Task OnBeforeNavigate_FiresAfterGuards()
    {
        // Guard runs first; if it Blocks, OnBeforeNavigate should NOT fire
        bool beforeFired = false;
        _beforeHandler = (_, _) => beforeFired = true;
        NavigationService.Instance.OnBeforeNavigate += _beforeHandler;

        var routes = new[]
        {
            new RouteDefinition("/", typeof(RootLayout),
                new RouteDefinition("dashboard", typeof(DashboardPage))
                {
                    Guard = _ => new GuardResult.Block()
                }
            )
        };
        var instance = CreateInstance(routes: routes);
        instance.NavigateTo("/dashboard");
        await instance.LastNavigationTask;

        beforeFired.Should().BeFalse();
    }

    [Fact]
    public async Task OnAfterNavigate_NotFired_WhenCancelledInBeforeEvent()
    {
        _beforeHandler = (_, args) => args.Cancel = true;
        NavigationService.Instance.OnBeforeNavigate += _beforeHandler;

        bool afterFired = false;
        _afterHandler = (_, _) => afterFired = true;
        NavigationService.Instance.OnAfterNavigate += _afterHandler;

        var instance = CreateInstance();
        instance.NavigateTo("/dashboard");
        await instance.LastNavigationTask;

        afterFired.Should().BeFalse();
    }

    [Fact]
    public async Task Reload_SetsIsReload_InBeforeNavigateArgs()
    {
        BeforeNavigationEventArgs? captured = null;
        _beforeHandler = (_, args) => captured = args;
        NavigationService.Instance.OnBeforeNavigate += _beforeHandler;

        var instance = CreateInstance();
        instance.NavigateTo("/dashboard");
        await instance.LastNavigationTask;

        instance.Reload();
        await instance.LastNavigationTask;

        // The LAST captured event is from Reload
        captured.Should().NotBeNull();
        captured!.IsReload.Should().BeTrue();
    }

    [Fact]
    public async Task Reload_SetsIsReload_InAfterNavigateArgs()
    {
        NavigationEventArgs? captured = null;
        _afterHandler = (_, args) => captured = args;
        NavigationService.Instance.OnAfterNavigate += _afterHandler;

        var instance = CreateInstance();
        instance.NavigateTo("/dashboard");
        await instance.LastNavigationTask;

        instance.Reload();
        await instance.LastNavigationTask;

        captured!.IsReload.Should().BeTrue();
    }

    [Fact]
    public async Task NavigationEvents_ContainRouterName()
    {
        NavigationEventArgs? afterArgs = null;
        _afterHandler = (_, args) => afterArgs = args;
        NavigationService.Instance.OnAfterNavigate += _afterHandler;

        var instance = CreateInstance(name: "my-router");
        instance.NavigateTo("/dashboard");
        await instance.LastNavigationTask;

        afterArgs!.RouterName.Should().Be("my-router");
    }

    [Fact]
    public async Task NavigateTo_RegularNavigation_IsReload_False()
    {
        NavigationEventArgs? captured = null;
        _afterHandler = (_, args) => captured = args;
        NavigationService.Instance.OnAfterNavigate += _afterHandler;

        var instance = CreateInstance();
        instance.NavigateTo("/dashboard");
        await instance.LastNavigationTask;

        captured!.IsReload.Should().BeFalse();
    }
}
