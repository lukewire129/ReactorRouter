namespace ReactorRouterSample.Pages;

public class LoginPageState
{
    public string Username { get; set; } = "";
}

public class LoginPage : Component<LoginPageState>
{
    public override VisualNode Render()
      => ContentView(
          VStack(
              Label("🔐 Login")
                  .FontSize(32)
                  .FontAttributes(Microsoft.Maui.Controls.FontAttributes.Bold)
                  .HCenter(),
              Label("This page is outside the Dashboard layout.")
                  .FontSize(14)
                  .TextColor(Microsoft.Maui.Graphics.Colors.Gray)
                  .HCenter(),
              Entry()
                  .Placeholder("Username")
                  .Text(State.Username)
                  .OnTextChanged((_, e) => SetState(s => s.Username = e.NewTextValue)),
              Button("ReloadPage")
                  .OnClicked(() => NavigationService.Instance.Reload())
                  .BackgroundColor(Microsoft.Maui.Graphics.Color.FromArgb("#512BD4"))
                  .TextColor(Microsoft.Maui.Graphics.Colors.White)
                  .CornerRadius(8),
              Button("Back to Dashboard")
                  .OnClicked(() => NavigationService.Instance.NavigateTo("/dashboard"))
                  .BackgroundColor(Microsoft.Maui.Graphics.Color.FromArgb("#512BD4"))
                  .TextColor(Microsoft.Maui.Graphics.Colors.White)
                  .CornerRadius(8)
          )
          .Spacing(16)
          .Padding(48)
          .VCenter()
      );
}
