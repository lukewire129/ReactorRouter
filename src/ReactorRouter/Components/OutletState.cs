namespace ReactorRouter.Components;

public class OutletState
{
    /// <summary>Currently displayed component type.</summary>
    public Type? CurrentType { get; set; }

    /// <summary>Previous component type — kept alive during exit animation.</summary>
    public Type? PreviousType { get; set; }

    /// <summary>Route params merged from the full match chain.</summary>
    public RouteParams Params { get; set; } = RouteParams.Empty;

    /// <summary>Query params from the current path.</summary>
    public RouteQuery Query { get; set; } = RouteQuery.Empty;

    /// <summary>True while enter/exit animation is running.</summary>
    public bool IsAnimating { get; set; }

    /// <summary>Animation type for regular navigations.</summary>
    public TransitionType Transition { get; set; } = TransitionType.Fade;

    /// <summary>Animation type used when Reload() triggers this outlet. Defaults to None (instant).</summary>
    public TransitionType ReloadTransition { get; set; } = TransitionType.None;

    /// <summary>Animation duration in milliseconds.</summary>
    public int AnimationDuration { get; set; } = 300;

    /// <summary>Depth assigned by NavigationService when this outlet was registered. -1 = not registered.</summary>
    public int AssignedDepth { get; set; } = -1;

    /// <summary>Name of the Router that owns this outlet.</summary>
    public string RouterName { get; set; } = "default";
}
