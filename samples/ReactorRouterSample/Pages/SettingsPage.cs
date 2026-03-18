namespace ReactorRouterSample.Pages;

public class SettingsPageState
{
    public bool DarkMode { get; set; }
    public bool Notifications { get; set; } = true;
}

public class SettingsPage : Component<SettingsPageState>
{
    public override VisualNode Render()
       => VStack (
           Label ("⚙️ Settings")
               .FontSize (28)
               .FontAttributes (Microsoft.Maui.Controls.FontAttributes.Bold),

           // Dark mode toggle
           HStack (
               Label ("Dark Mode")
                   .FontSize (16)
                   .VCenter ()
                   .HFill (),
               Switch ()
                   .IsToggled (State.DarkMode)
                   .OnToggled ((_, e) => SetState (s => s.DarkMode = e.Value))
           )
           .Padding (0, 8),

           // Notifications toggle
           HStack (
               Label ("Notifications")
                   .FontSize (16)
                   .VCenter ()
                   .HFill (),
               Switch ()
                   .IsToggled (State.Notifications)
                   .OnToggled ((_, e) => SetState (s => s.Notifications = e.Value))
           )
           .Padding (0, 8),

           Label ($"Dark mode: {State.DarkMode} | Notifications: {State.Notifications}")
               .FontSize (12)
               .TextColor (Microsoft.Maui.Graphics.Colors.Gray)
               .Margin (0, 16, 0, 0)
       )
       .Spacing (8)
       .Padding (32);
}
