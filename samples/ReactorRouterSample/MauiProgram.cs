using ReactorRouter;
using ReactorRouterSample.Components;
using ReactorRouterSample.Resources.Styles;

namespace ReactorRouterSample
{
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {
            var builder = MauiApp.CreateBuilder();
            builder
                .UseMauiReactorApp<MainPage>(app =>
                    {
                        app.UseTheme<ApplicationTheme>();
                    },
                    unhandledExceptionAction: e =>
                    {
                        System.Diagnostics.Debug.WriteLine(e.ExceptionObject);
                    })
#if DEBUG
                .UseMauiReactorHotReload()
#endif
                .UseReactorRouter(r =>
                {
                    r.Routes(
                        new RouteDefinition ("/", typeof (RootLayout),
                            // Child routes render inside RootLayout's Outlet
                            new RouteDefinition ("dashboard", typeof (DashboardLayout),
                                RouteDefinition.Index (typeof (HomePage)),
                                new RouteDefinition ("settings", typeof (SettingsPage))
                                { Transition = TransitionType.SlideLeft },
                                new RouteDefinition ("profile/:userId", typeof (ProfilePage))
                                { Transition = TransitionType.Fade }
                            ),
                            new RouteDefinition ("login", typeof (LoginPage))
                            { Transition = TransitionType.Fade },
                            new RouteDefinition ("*", typeof (NotFoundPage)) // Fallback route
                        )
                    );
                    r.InitialPath("/dashboard");
                })
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                    fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
                });


            return builder.Build();
        }
    }
}
