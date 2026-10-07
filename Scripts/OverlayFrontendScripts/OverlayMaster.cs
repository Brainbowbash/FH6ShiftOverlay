// SPDX-License-Identifier: GPL-3.0-or-later
// SPDX-FileCopyrightText: 2026 Brainbowbash
using Godot;
using System.Diagnostics;
using System.Linq;
namespace OverlayUI;
public partial class OverlayMaster : Control
{
	public static OverlayMaster Instance { get; private set; }

	private HUDSafeFrame HUD;
	private PowerbandZone PowerbandZone;

	private FullPowerDisplay FullPowerDisplay;
	private SimplePowerDisplay SimplePowerDisplay;

	private ShiftIndicator CentralShiftIndicator;
	private ShiftIndicator RevGaugeShiftIndicator;

	private Control InitialNotice;

	private Label Desc;

	private bool isActive;

	private bool shown;
	public void Display(bool show)
	{
		shown = show;
		if (show)
		{
			HUD.Show();
		}
		else
		{
			HUD.Hide();
		}
	}

	private int _parentPID = -1;

	public override void _Ready()
	{
		Instance = this;

		int.TryParse(OS.GetCmdlineUserArgs().Where(e => e.StartsWith("--parent-pid=")).ToArray()[0].Substring("--parent-pid=".Length), out _parentPID);
		try
		{
			Process parent = Process.GetProcessById(_parentPID);
			parent.EnableRaisingEvents = true;

			parent.Exited += (_, _) =>
			{
				Callable.From(() => { StopUI(); GetTree().Quit(); }).CallDeferred();
			};
		}
		catch
		{
			Callable.From(() => { StopUI(); GetTree().Quit(); }).CallDeferred();
		}

		Desc = (Label)FindChild("Desc", true);

		InitialNotice = (Control)FindChild("InitialNotice", true);
		HUD = (HUDSafeFrame)FindChild("HUD", true);

		PowerbandZone = (PowerbandZone)FindChild("PowerbandZoneUI", true);

		FullPowerDisplay = (FullPowerDisplay)FindChild("FullPowerDisplay", true);
		SimplePowerDisplay = (SimplePowerDisplay)FindChild("SimplePowerDisplay", true);

		CentralShiftIndicator = (ShiftIndicator)FindChild("CentralShiftIndicator", true);
		RevGaugeShiftIndicator = (ShiftIndicator)FindChild("RevGaugeShiftIndicator", true);

		Data.DataMaster.ShiftValueChange += () => { CentralShiftIndicator.UpdateShiftArrow(Data.DataMaster.ShiftDirection); RevGaugeShiftIndicator.UpdateShiftArrow(Data.DataMaster.ShiftDirection); };

		Data.DataMaster.PowerbandChange += () => { (float lower, float upper, bool certainty) = Data.DataMaster.PowerbandNormalised; PowerbandZone.DisplayArc(lower, upper, certainty); };

		Data.DataMaster.EngineCurveChange += () => { FullPowerDisplay.UpdateCurve(Data.DataMaster.EngineCurveNormalised, Data.DataMaster.HighestPower.Item1, Data.DataMaster.HighestTorque.Item1, Data.DataMaster.GaugeMaxRPM); SimplePowerDisplay.UpdateCurve(Data.DataMaster.EngineCurveNormalised, Data.DataMaster.HighestPower.Item1, Data.DataMaster.GaugeMaxRPM); };

		Data.DataMaster.EnginePeakValuesChange += () => { FullPowerDisplay.UpdatePeakValues(Data.DataMaster.HighestPower, Data.DataMaster.HighestTorque); };

		Data.DataMaster.CurrentPowerChange += () => { FullPowerDisplay.UpdateCurrentValues(Data.DataMaster.CurrentPower, Data.DataMaster.CurrentTorque); SimplePowerDisplay.UpdateCurrentValues(Data.DataMaster.CurrentPower); };

		Data.DataMaster.MenuChange += () => { Display(!Data.DataMaster.InMenu); };

		string udpDebug = OverlaySetup();

		if (udpDebug != "")
		{
			Desc.Text = udpDebug;
		}

		WindowHandler.OverlayWindowHandler.OverlayActivated += () => { Instance.StartUI(); };
	}

	public override void _Process(double delta)
	{
		if (isActive) Data.DataMaster.ProcessPackets();
	}

	public void StartUI()
	{
		InitialNotice.Hide();
		HUD.Show();

		isActive = true;
	}

	private string OverlaySetup()
	{
		Data.DataMaster.UpdateSettings(Config.PersistentStorage.DataOutIPPort.Value, Config.PersistentStorage.DataOutIPAddress.Value, Config.PersistentStorage.PowerUnit.Value, Config.PersistentStorage.TorqueUnit.Value);
		HUD.SetSafeFrame(Config.PersistentStorage.HUDSafeFrameHorizontal.Value, Config.PersistentStorage.HUDSafeFrameVertical.Value);

		CentralShiftIndicator.Display(Config.PersistentStorage.DisplayCentralShiftIndicator.Value);
		RevGaugeShiftIndicator.Display(Config.PersistentStorage.DisplayRevGaugeShiftIndicator.Value);

		PowerbandZone.Display(Config.PersistentStorage.DisplayRevGaugePowerbandZone.Value);

		FullPowerDisplay.SetUnits(Config.PersistentStorage.PowerUnit.Value, Config.PersistentStorage.TorqueUnit.Value);
		SimplePowerDisplay.SetUnits(Config.PersistentStorage.PowerUnit.Value);

		switch (Config.PersistentStorage.DisplayRevGaugeEnginePowerInfo.Value)
		{
			case Enums.PowerCurveOverlayType.Disabled:
				FullPowerDisplay.Display(false);
				SimplePowerDisplay.Display(false);
				break;

			case Enums.PowerCurveOverlayType.Simple:
				FullPowerDisplay.Display(false);
				SimplePowerDisplay.Display(true);
				break;

			case Enums.PowerCurveOverlayType.Full:
				FullPowerDisplay.Display(true);
				SimplePowerDisplay.Display(false);
				break;
		}
		return Data.DataMaster.BindUDP();
	}

	public void StopUI()
	{
		isActive = false;

		Data.DataMaster.CloseUDP();
	}
}

