using Godot;
namespace OverlayUI;
public partial class HUDSafeFrame : MarginContainer
{
	public override void _Ready()
	{
		SetSafeFrame(5.2, 9.3);
	}
	public void SetSafeFrame(double horizontalSafeFrame, double verticalSafeFrame)
	{
		AnchorTop = (float)(verticalSafeFrame / 200.0);
		AnchorBottom = (float)(1.0 - verticalSafeFrame / 200.0);

		AnchorLeft = (float)(horizontalSafeFrame / 200.0);
		AnchorRight = (float)(1.0 - horizontalSafeFrame / 200.0);
	}
}
