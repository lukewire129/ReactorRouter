namespace ReactorRouter.Components;

/// <summary>
/// Navigation trigger component. Wraps child content in a TapGestureRecognizer
/// that calls NavigationService.NavigateTo on tap.
///
/// Usage (default router):
/// <code>
/// new Link().To("/dashboard/settings")
/// </code>
///
/// Usage (named router):
/// <code>
/// new Link().To("/item/42").Router("detail")
/// </code>
/// </summary>
public partial class Link : Component
{
    [Prop] private string _to = "/";
    [Prop] private string _router = "default";
    [Prop] private VisualNode? _child;

    public override VisualNode Render()
    {
        return ContentView(
            _child ?? Label(_to)
                .TextColor(Microsoft.Maui.Graphics.Colors.Blue)
                .TextDecorations(Microsoft.Maui.TextDecorations.Underline)
        )
        .OnTapped(() => NavigationService.Instance.NavigateTo(_router, _to));
    }
}
