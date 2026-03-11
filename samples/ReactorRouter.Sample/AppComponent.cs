using Microsoft.Maui.Controls.Internals;

namespace ReactorRouter.Sample;

/// <summary>
/// Route definitions:
///
/// /
///   dashboard/
///     (index) → HomePage
///     settings → SettingsPage (SlideLeft)
///     profile/:userId → ProfilePage (Fade)
///   login → LoginPage
///   * → NotFoundPage
/// </summary>
public class AppComponent : Component
{
    private static readonly RouteDefinition[] Routes =
    [
        new RouteDefinition("/", typeof(RootLayout),
            new RouteDefinition("dashboard", typeof(DashboardLayout),
                RouteDefinition.Index(typeof(HomePage)),
                new RouteDefinition("settings", typeof(SettingsPage))
                    { Transition = TransitionType.SlideLeft },
                new RouteDefinition("profile/:userId", typeof(ProfilePage))
                    { Transition = TransitionType.Fade }
            ),
            new RouteDefinition("login", typeof(LoginPage))
                { Transition = TransitionType.Fade },
            new RouteDefinition("*", typeof(NotFoundPage))
        )
    ];

    public override VisualNode Render()
        => ContentPage(new Router()
            .Routes(Routes)
            .InitialPath("/dashboard"));
}
