using Godot;
namespace MainMenu.OverlayControlButton;
public partial class OverlayControlButton : Button
{
	public override void _Ready()
	{
		Toggled += (state) => Text = state ? "Disable" : "Enable";
	}
}
