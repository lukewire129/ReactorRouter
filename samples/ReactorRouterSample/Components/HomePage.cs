namespace ReactorRouterSample.Components;

public class HomePage : Component
{
    public override VisualNode Render()
        => VStack (
            Label ("🏠 Home")
                .FontSize (28)
                .FontAttributes (Microsoft.Maui.Controls.FontAttributes.Bold)
                .HCenter (),
            Label ("Welcome to ReactorRouter!")
                .FontSize (16)
                .TextColor (Microsoft.Maui.Graphics.Colors.Gray)
                .HCenter (),
            Label ("Navigate using the sidebar →")
                .FontSize (14)
                .TextColor (Microsoft.Maui.Graphics.Colors.LightSlateGray)
                .HCenter ()
                .Margin (0, 8, 0, 0)
        )
        .Spacing (12)
        .Padding (32)
        .VCenter ()
        .HCenter ();
}
