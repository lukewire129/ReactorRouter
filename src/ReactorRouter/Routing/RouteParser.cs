namespace ReactorRouter.Routing;

/// <summary>
/// Parses a navigation path into path segments and query parameters.
/// </summary>
public static class RouteParser
{
    /// <summary>
    /// Splits a full path like "/dashboard/settings?theme=dark" into
    /// (segments: ["dashboard", "settings"], query: { theme: "dark" }).
    /// </summary>
    public static (string[] Segments, RouteQuery Query) Parse(string path)
    {
        ArgumentNullException.ThrowIfNull(path);

        var queryStart = path.IndexOf('?');
        string pathPart = queryStart >= 0 ? path[..queryStart] : path;
        string queryPart = queryStart >= 0 ? path[(queryStart + 1)..] : string.Empty;

        var segments = pathPart
            .Split('/', StringSplitOptions.RemoveEmptyEntries);

        var query = ParseQuery(queryPart);
        return (segments, query);
    }

    /// <summary>Parses "key=value&amp;key2=value2" into a RouteQuery dictionary.</summary>
    public static RouteQuery ParseQuery(string queryString)
    {
        var dict = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

        if (string.IsNullOrWhiteSpace(queryString))
            return new RouteQuery(dict);

        foreach (var part in queryString.Split('&', StringSplitOptions.RemoveEmptyEntries))
        {
            var eq = part.IndexOf('=');
            if (eq <= 0) continue;
            var key = Uri.UnescapeDataString(part[..eq]);
            var val = Uri.UnescapeDataString(part[(eq + 1)..]);
            dict[key] = val;
        }

        return new RouteQuery(dict);
    }

    /// <summary>
    /// Returns true if a route pattern segment matches a path segment,
    /// and extracts the parameter name/value if dynamic.
    /// </summary>
    public static bool TryMatchSegment(
        string pattern,
        string segment,
        out string? paramName,
        out string? paramValue)
    {
        if (pattern.StartsWith(':'))
        {
            paramName = pattern[1..];
            paramValue = segment;
            return true;
        }

        if (pattern == "*")
        {
            paramName = null;
            paramValue = null;
            return true;
        }

        paramName = null;
        paramValue = null;
        return string.Equals(pattern, segment, StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>Score for a route segment (higher = more specific).</summary>
    public static int ScoreSegment(string pattern) => pattern switch
    {
        "*" => 1,
        _ when pattern.StartsWith(':') => 2,
        _ => 3
    };
}
