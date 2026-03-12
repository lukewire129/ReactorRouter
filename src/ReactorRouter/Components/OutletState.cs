namespace ReactorRouter.Components;

public class OutletState
{
    /// <summary>Currently displayed component type.</summary>
    public Type? CurrentType { get; set; }

    /// <summary>Previous component type — kept alive during animation.</summary>
    public Type? PreviousType { get; set; }

    /// <summary>Route params merged from the full match chain.</summary>
    public RouteParams Params { get; set; } = RouteParams.Empty;

    /// <summary>Query params from the current path.</summary>
    public RouteQuery Query { get; set; } = RouteQuery.Empty;

    /// <summary>True while enter/exit animation is running.</summary>
    public bool IsAnimating { get; set; }

    /// <summary>Animation type for this outlet (can be overridden per-route).</summary>
    public TransitionType Transition { get; set; } = TransitionType.Fade;

    /// <summary>Animation duration in milliseconds.</summary>
    public int AnimationDuration { get; set; } = 300;

    /// <summary>Depth assigned by NavigationService when this outlet was registered. -1 = not registered.</summary>
    public int AssignedDepth { get; set; } = -1;
}