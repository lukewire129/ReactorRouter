namespace ReactorRouter.Navigation;

/// <summary>
/// Context passed to a route guard when evaluating a navigation request.
/// </summary>
public class RouteGuardContext
{
    /// <summary>The path being navigated away from.</summary>
    public string FromPath { get; init; } = "/";

    /// <summary>The path being navigated to.</summary>
    public string ToPath { get; init; } = "/";

    /// <summary>Merged route parameters for the target path.</summary>
    public RouteParams Params { get; init; } = RouteParams.Empty;

    /// <summary>Query string parameters for the target path.</summary>
    public RouteQuery QueryParams { get; init; } = RouteQuery.Empty;
}
