namespace ReactorRouter.Sample;

/// <summary>
/// Root layout — wraps the entire app.
/// Depth 0: renders DashboardLayout or LoginPage depending on route.
/// </summary>
public class RootLayout : Component
{
    public override VisualNode Render()
        => ContentView(
            // Outlet at depth 0 renders DashboardLayout or LoginPage
            new Outlet()
                .Transition(TransitionType.Fade)
                .Duration(250)
        );
}
