namespace ReactorRouter.Navigation;

/// <summary>
/// Represents the result of a route guard evaluation.
/// </summary>
public abstract record GuardResult
{
    /// <summary>Allow navigation to proceed.</summary>
    public sealed record Allow : GuardResult;

    /// <summary>Redirect navigation to a different path.</summary>
    public sealed record Redirect(string Path) : GuardResult;

    /// <summary>Block navigation entirely (no redirect, no events).</summary>
    public sealed record Block : GuardResult;
}
