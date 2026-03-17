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
                .UseMauiReactorHotReload()
                .UseReactorRouter(r =>
                {
                    r.Routes(
                        new("/", typeof(RootLayout),
                            new("dashboard", typeof(DashboardLayout),
                                RouteDefinition.Index(typeof(HomePage)),
                                new("settings", typeof(SettingsPage))
                                    { Transition = TransitionType.SlideLeft },
                                new("profile/:userId", typeof(ProfilePage))
                                    { Transition = TransitionType.Fade }
                            ),
                            new("login", typeof(LoginPage))
                                { Transition = TransitionType.Fade },
                            new("*", typeof(NotFoundPage))
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
