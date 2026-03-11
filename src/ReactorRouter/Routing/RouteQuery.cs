namespace ReactorRouter.Routing;

/// <summary>Query string parameters from ?key=value pairs.</summary>
public sealed class RouteQuery
{
    private readonly IReadOnlyDictionary<string, string> _store;

    public static readonly RouteQuery Empty = new(new Dictionary<string, string>());

    public RouteQuery(IReadOnlyDictionary<string, string> store) => _store = store;

    public string Get(string key) =>
        _store.TryGetValue(key, out var v) ? v
            : throw new KeyNotFoundException($"Query param '{key}' not found.");

    public T Get<T>(string key) where T : IConvertible =>
        (T)Convert.ChangeType(Get(key), typeof(T));

    public string GetOrDefault(string key, string defaultValue = "") =>
        _store.TryGetValue(key, out var v) ? v : defaultValue;

    public T GetOrDefault<T>(string key, T defaultValue = default!) where T : IConvertible =>
        _store.TryGetValue(key, out var v)
            ? (T)Convert.ChangeType(v, typeof(T))
            : defaultValue;

    public bool Contains(string key) => _store.ContainsKey(key);
}
