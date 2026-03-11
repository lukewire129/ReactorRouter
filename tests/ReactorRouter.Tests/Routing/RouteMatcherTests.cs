namespace ReactorRouter.Tests.Routing;

// Dummy component types used in tests
file class RootLayout;
file class DashboardLayout;
file class HomePage;
file class SettingsPage;
file class ProfilePage;
file class LoginPage;
file class NotFoundPage;

public class RouteMatcherTests
{
    // Reusable route tree for most tests:
    // /
    //   dashboard/
    //     (index) → HomePage
    //     settings → SettingsPage
    //     profile/:userId → ProfilePage
    //   login → LoginPage
    //   * → NotFoundPage
    private static readonly IReadOnlyList<RouteDefinition> Routes =
    [
        new RouteDefinition("/", typeof(RootLayout),
            new RouteDefinition("dashboard", typeof(DashboardLayout),
                RouteDefinition.Index(typeof(HomePage)),
                new RouteDefinition("settings", typeof(SettingsPage)),
                new RouteDefinition("profile/:userId", typeof(ProfilePage))
            ),
            new RouteDefinition("login", typeof(LoginPage)),
            new RouteDefinition("*", typeof(NotFoundPage))
        )
    ];

    [Fact]
    public void Match_Root_ReturnsRootLayout()
    {
        var chain = RouteMatcher.Match("/", Routes);
        chain.Should().NotBeNull();
        chain![0].ComponentType.Should().Be(typeof(RootLayout));
    }

    [Fact]
    public void Match_Dashboard_ReturnsTwoLevels()
    {
        var chain = RouteMatcher.Match("/dashboard", Routes);
        chain.Should().NotBeNull().And.HaveCount(3); // Root + Dashboard + Index(Home)
        chain![0].ComponentType.Should().Be(typeof(RootLayout));
        chain[1].ComponentType.Should().Be(typeof(DashboardLayout));
        chain[2].ComponentType.Should().Be(typeof(HomePage)); // index
    }

    [Fact]
    public void Match_Settings_ReturnsThreeLevels()
    {
        var chain = RouteMatcher.Match("/dashboard/settings", Routes);
        chain.Should().NotBeNull().And.HaveCount(3);
        chain![2].ComponentType.Should().Be(typeof(SettingsPage));
    }

    [Fact]
    public void Match_Profile_ExtractsParam()
    {
        var chain = RouteMatcher.Match("/dashboard/profile/42", Routes);
        chain.Should().NotBeNull().And.HaveCount(3);
        chain![2].ComponentType.Should().Be(typeof(ProfilePage));
        chain[2].Params.Get("userId").Should().Be("42");
    }

    [Fact]
    public void Match_Login_ReturnsLoginPage()
    {
        var chain = RouteMatcher.Match("/login", Routes);
        chain.Should().NotBeNull();
        chain!.Last().ComponentType.Should().Be(typeof(LoginPage));
    }

    [Fact]
    public void Match_Unknown_ReturnsWildcard()
    {
        var chain = RouteMatcher.Match("/some/unknown/path", Routes);
        chain.Should().NotBeNull();
        chain!.Last().ComponentType.Should().Be(typeof(NotFoundPage));
    }

    [Fact]
    public void Match_DepthsAreAssignedCorrectly()
    {
        var chain = RouteMatcher.Match("/dashboard/settings", Routes)!;
        chain[0].Depth.Should().Be(0);
        chain[1].Depth.Should().Be(1);
        chain[2].Depth.Should().Be(2);
    }

    [Fact]
    public void Match_StaticBeatsParam_SameLevel()
    {
        // When both "settings" (static) and ":slug" (param) could match "settings",
        // static should win
        var routes = new List<RouteDefinition>
        {
            new(":slug", typeof(ProfilePage)),
            new("settings", typeof(SettingsPage))
        };

        var chain = RouteMatcher.Match("/settings", routes);
        chain.Should().NotBeNull();
        chain!.Last().ComponentType.Should().Be(typeof(SettingsPage));
    }

    [Fact]
    public void Match_ReturnsNull_WhenNoMatch()
    {
        var routes = new List<RouteDefinition>
        {
            new("dashboard", typeof(DashboardLayout))
        };
        var chain = RouteMatcher.Match("/nonexistent", routes);
        chain.Should().BeNull();
    }

    [Fact]
    public void Match_ParamsMergedAcrossDepths()
    {
        var routes = new List<RouteDefinition>
        {
            new RouteDefinition(":orgId", typeof(RootLayout),
                new RouteDefinition(":userId", typeof(ProfilePage))
            )
        };

        var chain = RouteMatcher.Match("/acme/42", routes)!;
        chain.Should().NotBeNull();
        var leaf = chain.Last();
        leaf.Params.Get("orgId").Should().Be("acme");
        leaf.Params.Get("userId").Should().Be("42");
    }
}
