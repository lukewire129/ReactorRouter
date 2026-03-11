namespace ReactorRouter.Navigation;

/// <summary>
/// Cascading context provided by Router and read by child components via [Param].
/// Contains the full match chain, merged params, and query for the current navigation.
/// </summary>
/// <summary>
/// IParameter&lt;T&gt; requires T : new() and mutable properties for Set(Action&lt;T&gt;) mutation.
/// </summary>
public sealed class RouterContext
{
    public static readonly RouterContext Empty = new();

    public RouteMatch[] MatchChain { get; set; } = [];
    public RouteParams Params { get; set; } = RouteParams.Empty;
    public RouteQuery Query { get; set; } = RouteQuery.Empty;
    public string CurrentPath { get; set; } = "/";
}
