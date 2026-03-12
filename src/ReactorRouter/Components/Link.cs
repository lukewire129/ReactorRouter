namespace ReactorRouter.Components;

/// <summary>
/// Navigation trigger component. Wraps child content in a TapGestureRecognizer
/// that calls NavigationService.NavigateTo(To) on tap.
///
/// Usage:
/// <code>
/// new Link().To("/dashboard/settings")
/// </code>
/// </summary>
public partial class Link : Component
{
    [Prop] private string _to = "/";
    [Prop] private VisualNode? _child;

    public override VisualNode Render()
    {
        return ContentView(
            _child ?? Label(_to)
                .TextColor(Microsoft.Maui.Graphics.Colors.Blue)
                .TextDecorations(Microsoft.Maui.TextDecorations.Underline)
        )
        .OnTapped(() => NavigationService.Instance.NavigateTo(_to));
    }
}
