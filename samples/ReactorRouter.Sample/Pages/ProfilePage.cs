namespace ReactorRouter.Sample;

/// <summary>
/// Demo: reading route params (:userId) and query params (?tab=posts) via [Param] RouterContext.
/// Navigate to /dashboard/profile/42?tab=posts to see params in action.
/// </summary>
public partial class ProfilePage : Component
{
    [Param] IParameter<RouterContext> _ctx;

    public override VisualNode Render()
    {
        var userId = _ctx.Value.Params.GetOrDefault("userId", "unknown");
        var tab = _ctx.Value.Query.GetOrDefault("tab", "overview");
        var currentPath = _ctx.Value.CurrentPath;

        return VStack(
            Label("👤 Profile")
                .FontSize(28)
                .FontAttributes(Microsoft.Maui.Controls.FontAttributes.Bold),

            // Route param demo
            Border(
                VStack(
                    Label("Route Parameters")
                        .FontSize(12)
                        .TextColor(Microsoft.Maui.Graphics.Colors.Gray),
                    Label($"userId = \"{userId}\"")
                        .FontSize(18)
                        .FontAttributes(Microsoft.Maui.Controls.FontAttributes.Bold)
                        .TextColor(Microsoft.Maui.Graphics.Color.FromArgb("#512BD4"))
                )
                .Spacing(4)
                .Padding(12)
            )
            .BackgroundColor(Microsoft.Maui.Graphics.Color.FromArgb("#F0EBFF"))
            .Stroke(Microsoft.Maui.Graphics.Color.FromArgb("#512BD4"))
            .StrokeThickness(1),

            // Query param demo
            Border(
                VStack(
                    Label("Query Parameters")
                        .FontSize(12)
                        .TextColor(Microsoft.Maui.Graphics.Colors.Gray),
                    Label($"tab = \"{tab}\"")
                        .FontSize(18)
                        .FontAttributes(Microsoft.Maui.Controls.FontAttributes.Bold)
                        .TextColor(Microsoft.Maui.Graphics.Color.FromArgb("#0078D4"))
                )
                .Spacing(4)
                .Padding(12)
            )
            .BackgroundColor(Microsoft.Maui.Graphics.Color.FromArgb("#EBF4FF"))
            .Stroke(Microsoft.Maui.Graphics.Color.FromArgb("#0078D4"))
            .StrokeThickness(1),

            // Tab buttons to demo query param change
            HStack(
                TabButton("overview", tab, userId),
                TabButton("posts", tab, userId),
                TabButton("settings", tab, userId)
            )
            .Spacing(8),

            Label($"Path: {currentPath}")
                .FontSize(11)
                .TextColor(Microsoft.Maui.Graphics.Colors.Gray)
        )
        .Spacing(12)
        .Padding(32);
    }

    private static VisualNode TabButton(string tabName, string currentTab, string userId)
        => Button(tabName)
            .OnClicked(() => NavigationService.Instance.NavigateTo($"/dashboard/profile/{userId}?tab={tabName}"))
            .BackgroundColor(tabName == currentTab
                ? Microsoft.Maui.Graphics.Color.FromArgb("#512BD4")
                : Microsoft.Maui.Graphics.Colors.LightGray)
            .TextColor(tabName == currentTab
                ? Microsoft.Maui.Graphics.Colors.White
                : Microsoft.Maui.Graphics.Colors.Black)
            .CornerRadius(6);
}
