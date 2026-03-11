namespace ReactorRouter.Routing;

/// <summary>
/// Represents a single matched route in the match chain.
/// </summary>
public sealed class RouteMatch
{
    public RouteMatch(
        RouteDefinition route,
        string matchedPath,
        RouteParams @params,
        int depth)
    {
        Route = route;
        MatchedPath = matchedPath;
        Params = @params;
        Depth = depth;
    }

    /// <summary>The route definition that was matched.</summary>
    public RouteDefinition Route { get; }

    /// <summary>The accumulated path matched so far (e.g. "/dashboard/settings").</summary>
    public string MatchedPath { get; }

    /// <summary>Route parameters extracted from path segments (e.g. :userId → "123").</summary>
    public RouteParams Params { get; }

    /// <summary>Nesting depth in the match chain (0 = root).</summary>
    public int Depth { get; }

    /// <summary>Component type to render at this depth.</summary>
    public Type ComponentType => Route.ComponentType;

    /// <summary>Transition animation defined on the route (may be null).</summary>
    public TransitionType? Transition => Route.Transition;
}
