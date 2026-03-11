namespace ReactorRouter.Routing;

/// <summary>Route path parameters extracted from :param segments.</summary>
public sealed class RouteParams
{
    private readonly IReadOnlyDictionary<string, string> _store;

    public static readonly RouteParams Empty = new(new Dictionary<string, string>());

    public RouteParams(IReadOnlyDictionary<string, string> store) => _store = store;

    public string Get(string key) =>
        _store.TryGetValue(key, out var v) ? v
            : throw new KeyNotFoundException($"Route param '{key}' not found.");

    public T Get<T>(string key) where T : IConvertible =>
        (T)Convert.ChangeType(Get(key), typeof(T));

    public string? GetOrDefault(string key, string? defaultValue = null) =>
        _store.TryGetValue(key, out var v) ? v : defaultValue;

    public T? GetOrDefault<T>(string key, T? defaultValue = default) where T : IConvertible =>
        _store.TryGetValue(key, out var v)
            ? (T)Convert.ChangeType(v, typeof(T))
            : defaultValue;

    public bool Contains(string key) => _store.ContainsKey(key);

    /// <summary>Merges two RouteParams — later params override earlier ones.</summary>
    public RouteParams MergeWith(RouteParams other)
    {
        var merged = new Dictionary<string, string>(_store);
        foreach (var kvp in other._store)
            merged[kvp.Key] = kvp.Value;
        return new RouteParams(merged);
    }
}
