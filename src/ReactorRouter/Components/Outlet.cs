using MauiReactor.Parameters;

namespace ReactorRouter.Components;

/// <summary>
/// Renders the matched child route at this nesting level, with animated content transitions.
/// Place Outlet() inside any layout component to define where child routes appear.
/// </summary>
public partial class Outlet : Component<OutletState>
{
    [Prop] private TransitionType _transition = TransitionType.Fade;
    [Prop] private int _duration = 300;

    /// <summary>
    /// Transition to use when <see cref="NavigationService.Reload()"/> triggers this outlet.
    /// Defaults to <see cref="TransitionType.None"/> (instant remount).
    /// </summary>
    [Prop] private TransitionType _reloadTransition = TransitionType.Fade;

    // Read the enclosing Router's context to determine which router this outlet belongs to.
    [Param] IParameter<RouterContext> _routerContext;

    protected override void OnMounted()
    {
        // Hot Reload 후 애니메이션 고착 방지
        if (State.IsAnimating)
            SetState(s => { s.PreviousType = null; s.IsAnimating = false; });

        var routerName = _routerContext.Value.RouterName;

        SetState(s =>
        {
            s.RouterName = routerName;
            s.Transition = _transition;
            s.ReloadTransition = _reloadTransition;
            s.AnimationDuration = _duration;
            s.AssignedDepth = NavigationService.Instance.RegisterOutlet(
                routerName, new OutletRegistration(UpdateRoute));
        });
    }

    protected override void OnWillUnmount()
    {
        if (State.AssignedDepth >= 0)
            NavigationService.Instance.UnregisterOutlet(State.RouterName, State.AssignedDepth);
    }

    internal void UpdateRoute(Type? componentType, RouteParams @params, RouteQuery query, bool forceRemount = false)
    {
        bool isSameType = componentType?.FullName == State.CurrentType?.FullName;

        // Same type and no forced remount → just update params/query
        if (isSameType && !forceRemount)
        {
            SetState(s => { s.Params = @params; s.Query = query; });
            return;
        }

        // Reload of same component type — use ReloadTransition
        if (forceRemount && isSameType)
        {
            var reloadTransition = State.ReloadTransition;
            var shouldAnimate = reloadTransition != TransitionType.None;
            var currentType = State.CurrentType;
            var animDuration = State.AnimationDuration;
            var originalTransition = State.Transition;

            SetState(s =>
            {
                s.PreviousType = shouldAnimate ? currentType : null;
                s.CurrentType = null;   // unmount current component
                s.Params = @params;
                s.Query = query;
                s.IsAnimating = shouldAnimate;
                if (shouldAnimate) s.Transition = reloadTransition;
            });

            int restoreDelay = shouldAnimate ? animDuration + 50 : 50;
            _ = RestoreAfterReloadAsync(componentType, restoreDelay, shouldAnimate ? originalTransition : null);
            return;
        }

        // Regular navigation to a different component type
        var previous = State.CurrentType;
        var animate = previous != null && componentType != null && State.Transition != TransitionType.None;

        SetState(s =>
        {
            s.PreviousType = previous;
            s.CurrentType = componentType;
            s.Params = @params;
            s.Query = query;
            s.IsAnimating = animate;
        });

        if (animate)
            _ = ClearAnimationAfterDelayAsync(State.AnimationDuration + 50);
    }

    private async Task RestoreAfterReloadAsync(Type? componentType, int delay, TransitionType? originalTransition)
    {
        await Task.Delay(delay).ConfigureAwait(false);
        SetState(s =>
        {
            s.PreviousType = null;
            s.IsAnimating = false;
            s.CurrentType = componentType;
            if (originalTransition.HasValue) s.Transition = originalTransition.Value;
        });
    }

    private async Task ClearAnimationAfterDelayAsync(int delayMs)
    {
        await Task.Delay(delayMs).ConfigureAwait(false);
        SetState(s =>
        {
            s.PreviousType = null;
            s.IsAnimating = false;
        });
    }

    public override VisualNode Render()
    {
        return Grid(
            State.PreviousType != null
                ? Grid(CreateComponent(State.PreviousType))
                    .Opacity(State.IsAnimating ? 0.0 : 1.0)
                    .TranslationX(GetExitTranslationX())
                    .TranslationY(GetExitTranslationY())
                    .Scale(GetExitScale())
                    .WithAnimation(duration: State.AnimationDuration)
                : null,

            State.CurrentType != null
                ? Grid(CreateComponent(State.CurrentType))
                    .IsVisible(!State.IsAnimating)
                    .Opacity(GetEnterOpacity())
                    .TranslationX(GetEnterTranslationX())
                    .TranslationY(GetEnterTranslationY())
                    .Scale(GetEnterScale())
                    .WithAnimation(duration: State.AnimationDuration)
                : null
        );
    }

    private static VisualNode CreateComponent(Type type) =>
        ComponentFactory.Create(type);

    private double GetEnterOpacity() => State.Transition switch
    {
        TransitionType.Fade => State.IsAnimating ? 0.0 : 1.0,
        _ => 1.0
    };

    private double GetEnterTranslationX()
    {
        if (!State.IsAnimating) return 0;
        return State.Transition switch
        {
            TransitionType.SlideLeft => 300,
            TransitionType.SlideRight => -300,
            _ => 0
        };
    }

    private double GetEnterTranslationY()
    {
        if (!State.IsAnimating) return 0;
        return State.Transition switch
        {
            TransitionType.SlideUp => 300,
            TransitionType.SlideDown => -300,
            _ => 0
        };
    }

    private double GetEnterScale() => State.Transition switch
    {
        TransitionType.ZoomIn => State.IsAnimating ? 0.7 : 1.0,
        TransitionType.ZoomOut => State.IsAnimating ? 1.3 : 1.0,
        TransitionType.Scale => State.IsAnimating ? 0.85 : 1.0,
        _ => 1.0
    };

    private double GetExitTranslationX()
    {
        if (!State.IsAnimating) return 0;
        return State.Transition switch
        {
            TransitionType.SlideLeft => -300,
            TransitionType.SlideRight => 300,
            _ => 0
        };
    }

    private double GetExitTranslationY()
    {
        if (!State.IsAnimating) return 0;
        return State.Transition switch
        {
            TransitionType.SlideUp => -300,
            TransitionType.SlideDown => 300,
            _ => 0
        };
    }

    private double GetExitScale() => State.Transition switch
    {
        TransitionType.ZoomIn => State.IsAnimating ? 1.3 : 1.0,
        TransitionType.ZoomOut => State.IsAnimating ? 0.7 : 1.0,
        TransitionType.Scale => State.IsAnimating ? 1.15 : 1.0,
        _ => 1.0
    };
}
