namespace ReactorRouter;

/// <summary>
/// Registry for component factory functions, enabling AOT-safe component creation.
/// Components registered here are created via factory lambdas instead of Activator.CreateInstance.
/// </summary>
public static class ComponentRegistry
{
    private static readonly Dictionary<Type, Func<VisualNode>> _factories = new();

    /// <summary>
    /// Registers a component type using a default parameterless constructor factory.
    /// Use this for manual AOT-safe registration.
    /// </summary>
    public static void Register<T>() where T : VisualNode, new()
        => _factories[typeof(T)] = static () => new T();

    /// <summary>
    /// Registers a component type with a custom factory function.
    /// </summary>
    public static void Register(Type type, Func<VisualNode> factory)
        => _factories[type] = factory;

    internal static Func<VisualNode>? GetFactory(Type type)
        => _factories.TryGetValue(type, out var f) ? f : null;

    /// <summary>
    /// Clears all registered factories. Useful for testing.
    /// </summary>
    public static void Clear() => _factories.Clear();
}
