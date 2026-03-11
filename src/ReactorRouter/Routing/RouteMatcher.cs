namespace ReactorRouter.Routing;

/// <summary>
/// Matches a path string against a route tree and returns the best match chain.
/// Uses React Router v6 scoring: static(+3) > :param(+2) > *(+1).
/// </summary>
public static class RouteMatcher
{
    /// <summary>
    /// Matches <paramref name="path"/> against <paramref name="routes"/> and returns
    /// the best match chain from root to leaf, or null if no match.
    /// </summary>
    public static RouteMatch[]? Match(string path, IReadOnlyList<RouteDefinition> routes)
    {
        var (segments, _) = RouteParser.Parse(path);
        var best = FindBestMatch(segments, 0, routes, string.Empty, RouteParams.Empty);
        return best?.ToArray();
    }

    private static List<RouteMatch>? FindBestMatch(
        string[] segments,
        int segmentIndex,
        IReadOnlyList<RouteDefinition> routes,
        string accumulatedPath,
        RouteParams accumulatedParams)
    {
        List<RouteMatch>? bestChain = null;
        int bestScore = -1;

        foreach (var route in routes)
        {
            var result = TryMatchRoute(segments, segmentIndex, route, accumulatedPath, accumulatedParams);
            if (result is null) continue;

            var (chain, score) = result.Value;
            if (score > bestScore)
            {
                bestScore = score;
                bestChain = chain;
            }
        }

        return bestChain;
    }

    private static (List<RouteMatch> Chain, int Score)?
        TryMatchRoute(
            string[] segments,
            int segmentIndex,
            RouteDefinition route,
            string accumulatedPath,
            RouteParams accumulatedParams)
    {
        // Index route: matches when segments are exhausted at this level
        if (route.IsIndex)
        {
            if (segmentIndex != segments.Length) return null;
            var match = new RouteMatch(route, accumulatedPath, accumulatedParams, depth: 0);
            return (new List<RouteMatch> { match }, 3);
        }

        // Wildcard "*" matches any remaining path
        if (route.Path == "*")
        {
            var match = new RouteMatch(route, accumulatedPath, accumulatedParams, depth: 0);
            return (new List<RouteMatch> { match }, 1);
        }

        // Root "/" special case: matches empty segments
        if (route.Path == "/")
        {
            var newPath = "/";
            var childChain = FindBestMatch(
                segments, segmentIndex, route.Children, newPath, accumulatedParams);

            if (childChain is null) return null;

            var match = new RouteMatch(route, newPath, accumulatedParams, depth: 0);
            var chain = PrependAndReindex(match, childChain);
            return (chain, 3 + ScoreChain(childChain));
        }

        // Normal segment matching
        var patternSegments = route.Path.Split('/', StringSplitOptions.RemoveEmptyEntries);

        if (segmentIndex + patternSegments.Length > segments.Length) return null;

        var routeScore = 0;
        var extractedParams = new Dictionary<string, string>();

        for (int i = 0; i < patternSegments.Length; i++)
        {
            var pattern = patternSegments[i];
            var segment = segments[segmentIndex + i];

            if (!RouteParser.TryMatchSegment(pattern, segment, out var pName, out var pValue))
                return null;

            routeScore += RouteParser.ScoreSegment(pattern);
            if (pName is not null && pValue is not null)
                extractedParams[pName] = pValue;
        }

        var newAccumulatedPath = accumulatedPath.TrimEnd('/') + "/" +
                                 string.Join("/", segments[segmentIndex..(segmentIndex + patternSegments.Length)]);
        var newParams = extractedParams.Count > 0
            ? accumulatedParams.MergeWith(new RouteParams(extractedParams))
            : accumulatedParams;
        var nextIndex = segmentIndex + patternSegments.Length;

        // Exact match with no children needed
        if (nextIndex == segments.Length)
        {
            // Try to find an index child first
            var indexChild = route.Children.FirstOrDefault(c => c.IsIndex);
            if (indexChild is not null)
            {
                var indexMatch = new RouteMatch(indexChild, newAccumulatedPath, newParams, depth: 0);
                var parentMatch = new RouteMatch(route, newAccumulatedPath, newParams, depth: 0);
                var chain = PrependAndReindex(parentMatch, new List<RouteMatch> { indexMatch });
                return (chain, routeScore + 3);
            }

            // No index child — this route itself is the leaf
            if (route.Children.Count == 0 || route.ComponentType is not null)
            {
                var match = new RouteMatch(route, newAccumulatedPath, newParams, depth: 0);
                return (new List<RouteMatch> { match }, routeScore);
            }

            return null;
        }

        // More segments left — must match children
        if (route.Children.Count == 0) return null;

        var childResult = FindBestMatch(segments, nextIndex, route.Children, newAccumulatedPath, newParams);
        if (childResult is null) return null;

        var routeMatch = new RouteMatch(route, newAccumulatedPath, newParams, depth: 0);
        var fullChain = PrependAndReindex(routeMatch, childResult);
        return (fullChain, routeScore + ScoreChain(childResult));
    }

    /// <summary>Prepends a parent match and reassigns depth numbers.</summary>
    private static List<RouteMatch> PrependAndReindex(RouteMatch parent, List<RouteMatch> children)
    {
        var result = new List<RouteMatch>(children.Count + 1);
        result.Add(new RouteMatch(parent.Route, parent.MatchedPath, parent.Params, depth: 0));

        for (int i = 0; i < children.Count; i++)
        {
            var c = children[i];
            result.Add(new RouteMatch(c.Route, c.MatchedPath, c.Params, depth: i + 1));
        }

        return result;
    }

    private static int ScoreChain(List<RouteMatch> chain) =>
        chain.Sum(_ => 1);
}
