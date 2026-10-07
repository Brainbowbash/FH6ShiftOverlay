using Godot;
namespace MainMenu.SpinboxRateLimit;
public partial class SpinboxRateLimit : SpinBox
{
	private Timer updateTimer;
	public override void _Ready()
	{
		updateTimer = new Timer { Autostart = false, OneShot = true, WaitTime = 0.3f };
		AddChild(updateTimer);
		updateTimer.Timeout += () => EmitSignal(SignalName.PortCommitted, Value);
		ValueChanged += (double value) => updateTimer.Start(0.3f);
	}
	[Signal]
	public delegate void PortCommittedEventHandler(float value);

}
