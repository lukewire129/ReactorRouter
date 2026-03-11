using MauiReactor;

namespace ReactorRouter.Components;

/// <summary>
/// Renders the matched child route at this nesting level, with animated content transitions.
/// Place Outlet() inside any layout component to define where child routes appear.
/// </summary>
public partial class Outlet : Component<OutletState>
{
    private int _assignedDepth = -1;

    [Prop] private TransitionType _transition = TransitionType.Fade;
    [Prop] private int _duration = 300;

    protected override void OnMounted()
    {
        // Hot Reload 후 애니메이션 고착 방지
        if (State.IsAnimating)
            SetState(s => { s.PreviousType = null; s.IsAnimating = false; });

        SetState(s => { s.Transition = _transition; s.AnimationDuration = _duration; });

        _assignedDepth = NavigationService.Instance.RegisterOutlet(
            new OutletRegistration(UpdateRoute));
    }

    protected override void OnWillUnmount()
    {
        if (_assignedDepth >= 0)
            NavigationService.Instance.UnregisterOutlet(_assignedDepth);
    }

    internal void UpdateRoute(Type? componentType, RouteParams @params, RouteQuery query)
    {
        if (componentType == State.CurrentType)
        {
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
            Task.Delay(State.AnimationDuration + 50)
                .ContinueWith(_ => SetState(s =>
                {
                    s.PreviousType = null;
                    s.IsAnimating = false;
                }),
                TaskScheduler.FromCurrentSynchronizationContext());
        }
    }

    public override VisualNode Render()
    {
        // Grid 래퍼에 애니메이션 속성 적용 (IVisualElement 필요)
        return Grid(
            State.PreviousType != null
                ? Grid(CreateComponent(State.PreviousType))
                    .WithAnimation(duration: State.AnimationDuration)
                    .Opacity(State.IsAnimating ? 0.0 : 1.0)
                    .TranslationX(GetExitTranslationX())
                    .TranslationY(GetExitTranslationY())
                    .Scale(GetExitScale())
                : null,

            State.CurrentType != null
                ? Grid(CreateComponent(State.CurrentType))
                    .WithAnimation(duration: State.AnimationDuration)
                    .Opacity(GetEnterOpacity())
                    .TranslationX(GetEnterTranslationX())
                    .TranslationY(GetEnterTranslationY())
                    .Scale(GetEnterScale())
                : null
        );
    }

    private static VisualNode CreateComponent(Type type) =>
        (VisualNode)Activator.CreateInstance(type)!;

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
