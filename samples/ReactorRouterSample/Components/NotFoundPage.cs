using MauiReactor.Parameters;

namespace ReactorRouterSample.Components;

public partial class NotFoundPage : Component
{
    [Param] IParameter<RouterContext> _ctx;

    public override VisualNode Render()
        => VStack(
            Label("404")
                .FontSize(72)
                .FontAttributes(Microsoft.Maui.Controls.FontAttributes.Bold)
                .TextColor(Microsoft.Maui.Graphics.Colors.LightGray)
                .HCenter(),
            Label("Page Not Found")
                .FontSize(24)
                .HCenter(),
            Label($"Path: {_ctx.Value.CurrentPath}")
                .FontSize(13)
                .TextColor(Microsoft.Maui.Graphics.Colors.Gray)
                .HCenter(),
            Button("Go Home")
                .OnClicked(() => NavigationService.Instance.NavigateTo("/dashboard"))
                .BackgroundColor(Microsoft.Maui.Graphics.Color.FromArgb("#512BD4"))
                .TextColor(Microsoft.Maui.Graphics.Colors.White)
                .CornerRadius(8)
                .HCenter()
        )
        .Spacing(12)
        .Padding(32)
        .VCenter()
        .HCenter();
}
