namespace ReactorRouterSample.Components;

/// <summary>
/// Dashboard layout ??? custom sidebar + NavBar, Outlet for child routes.
/// This is the key demo: the sidebar stays while only the content area animates.
/// </summary>
public class DashboardLayout : Component
{
    public override VisualNode Render()
         => Grid (rows: "50, *", columns: "160, *",

             // ── Top NavBar (spans both columns) ──
             Border (
                 HStack (
                     Label ("ReactorRouter")
                         .FontSize (16)
                         .FontAttributes (Microsoft.Maui.Controls.FontAttributes.Bold)
                         .TextColor (Microsoft.Maui.Graphics.Colors.White)
                         .VCenter (),
                     Label ("Demo")
                         .FontSize (12)
                         .TextColor (Microsoft.Maui.Graphics.Colors.LightGray)
                         .VCenter ()
                         .Margin (8, 0, 0, 0)
                 )
                 .Spacing (0)
                 .Padding (16, 0)
             )
             .BackgroundColor (Microsoft.Maui.Graphics.Color.FromArgb ("#512BD4"))
             .GridColumnSpan (2),

             // ── Left Sidebar ──
             Border (
                 VStack (
                     NavButton ("🏠  Home", "/dashboard"),
                     NavButton ("⚙️  Settings", "/dashboard/settings"),
                     NavButton ("👤  Profile", "/dashboard/profile/42"),
                     NavButton ("🔐  Login page", "/login")
                 )
                 .Spacing (4)
                 .Padding (8)
             )
             .BackgroundColor (Microsoft.Maui.Graphics.Color.FromArgb ("#F5F5F5"))
             .GridRow (1),

             // ── Content area: child routes animate here ──
             new Outlet ()
                 .Transition (TransitionType.SlideLeft)
                 .Duration (300)
                 .GridRow (1)
                 .GridColumn (1)
         );

    private static VisualNode NavButton(string text, string path)
        => Button (text)
            .OnClicked (() => NavigationService.Instance.NavigateTo (path))
            .BackgroundColor (Microsoft.Maui.Graphics.Colors.Transparent)
            .TextColor (Microsoft.Maui.Graphics.Color.FromArgb ("#333333"))
            .HFill ()
            .HeightRequest (44);
}
