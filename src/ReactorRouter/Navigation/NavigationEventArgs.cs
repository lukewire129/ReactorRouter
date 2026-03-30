namespace ReactorRouter.Navigation;

/// <summary>
/// Event args for navigation lifecycle events (OnAfterNavigate).
/// </summary>
public class NavigationEventArgs : EventArgs
{
    /// <summary>The path navigated away from.</summary>
    public string FromPath { get; init; } = "/";

    /// <summary>The path navigated to.</summary>
    public string ToPath { get; init; } = "/";

    /// <summary>True when this navigation was triggered by Reload().</summary>
    public bool IsReload { get; init; }

    /// <summary>Name of the router that performed this navigation.</summary>
    public string RouterName { get; init; } = "default";
}

/// <summary>
/// Event args for the OnBeforeNavigate event. Set <see cref="Cancel"/> to true to abort navigation.
/// </summary>
public class BeforeNavigationEventArgs : NavigationEventArgs
{
    /// <summary>
    /// Set to true to cancel the navigation. Equivalent to Guard Block, but fires after guard evaluation.
    /// </summary>
    public bool Cancel { get; set; }
}
