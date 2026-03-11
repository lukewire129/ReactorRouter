namespace ReactorRouterSample.Components
{
    partial class MainPage : Component
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
            => ContentPage (new Router ()
                .Routes (Routes)
                .InitialPath ("/dashboard"));
    }
}
