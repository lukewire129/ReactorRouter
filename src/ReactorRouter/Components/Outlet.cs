namespace ReactorRouter.Components;

/// <summary>
/// Renders the matched child route at this nesting level, with animated content transitions.
/// Place Outlet() inside any layout component to define where child routes appear.
/// </summary>
public partial class Outlet : Component<OutletState>
{
    [Prop] private TransitionType _transition = TransitionType.Fade;
    [Prop] private int _duration = 300;

    protected override void OnMounted()
    {
        // Hot Reload 후 애니메이션 고착 방지
        if (State.IsAnimating)
            SetState(s => { s.PreviousType = null; s.IsAnimating = false; });

        SetState(s =>
        {
            s.Transition = _transition;
            s.AnimationDuration = _duration;
            s.AssignedDepth = NavigationService.Instance.RegisterOutlet(
                new OutletRegistration((type, @params, query, forceReload) =>
                    UpdateRoute(type, @params, query, forceReload)));
        });
    }

    protected override void OnWillUnmount()
    {
        if (State.AssignedDepth >= 0)
            NavigationService.Instance.UnregisterOutlet(State.AssignedDepth);
    }

    internal void UpdateRoute(Type? componentType, RouteParams @params, RouteQuery query, bool forceReload = false)
    {
        if (componentType?.FullName == State.CurrentType?.FullName)
        {
            if (forceReload && componentType != null)
            {
                var savedType = componentType;
                SetState(s => { s.CurrentType = null; s.Params = @params; s.Query = query; });
                MauiControls.Application.Current!.Dispatcher.Dispatch(() =>
                {
                    SetState(s => { s.CurrentType = savedType; });
                });
                return;
            }

            SetState(s => { s.Params = @params; s.Query = query; });
            return;
        }

        var previous = State.CurrentType;
        var shouldAnimate = previous != null && componentType != null
            && State.Transition != TransitionType.None;

        SetState(s =>
        {
            s.PreviousType = previous;
            s.CurrentType = componentType;
            s.Params = @params;
            s.Query = query;
            s.IsAnimating = shouldAnimate;
        });

        if (shouldAnimate)
        {
            _ = ClearAnimationAfterDelayAsync(State.AnimationDuration + 50);
        }
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
        // Grid 래퍼에 애니메이션 속성 적용 (IVisualElement 필요)
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
