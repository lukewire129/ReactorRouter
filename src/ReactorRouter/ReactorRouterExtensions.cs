using Microsoft.Maui.Hosting;

namespace ReactorRouter;

/// <summary>
/// Extension methods for integrating ReactorRouter into the MAUI application builder.
/// </summary>
public static class ReactorRouterExtensions
{
    /// <summary>
    /// Configures ReactorRouter with route definitions and component registration.
    /// Call this in MauiProgram.cs after UseMauiReactorApp().
    /// </summary>
    /// <example>
    /// builder.UseReactorRouter(r =>
    /// {
    ///     r.Routes(
    ///         new("/", typeof(RootLayout),
    ///             new("dashboard", typeof(DashboardPage))
    ///         )
    ///     );
    ///     r.InitialPath("/dashboard");
    /// });
    /// </example>
    public static MauiAppBuilder UseReactorRouter(
        this MauiAppBuilder builder,
        Action<ReactorRouterConfiguration> configure)
    {
        var config = new ReactorRouterConfiguration();
        configure(config);
        ReactorRouterConfig.Current = config;
        return builder;
    }
}

/// <summary>
/// Internal static storage for the ReactorRouter configuration set at app startup.
/// </summary>
internal static class ReactorRouterConfig
{
    internal static ReactorRouterConfiguration? Current { get; set; }
}
