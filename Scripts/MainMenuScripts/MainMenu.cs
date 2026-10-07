// SPDX-License-Identifier: GPL-3.0-or-later
// SPDX-FileCopyrightText: 2026 Brainbowbash
using Godot;
using System;
using System.Diagnostics;


namespace MainMenu;
public partial class MainMenu : Control
{
	private OverlayControlButton.OverlayControlButton enableToggleButton;

	private Process OverlayProcess;

	public override void _Ready()
	{

		foreach (Config.PersistentStorage.IConfigProperty property in Config.PersistentStorage.ConfigProperties)
		{
			SetUIValue(property);
		}

		enableToggleButton = (OverlayControlButton.OverlayControlButton)FindChild("OverlayControlButton");

		GetWindow().Borderless = false;
		GetWindow().Title = "Shift Overlay Configurator";

	}

	private void SetUIValue(Config.PersistentStorage.IConfigProperty property)
	{
		Node uiNode = FindChild(property.Key);

		switch (uiNode)
		{
			case CheckButton checkButton:
				checkButton.ButtonPressed = property.VariantValue.AsBool();
				break;

			case OptionButton optionButton:
				optionButton.Selected = property.VariantValue.AsInt32();
				break;

			case LineEdit lineEdit:
				lineEdit.Text = property.VariantValue.AsString();
				break;

			case SpinBox spinBox:
				spinBox.Value = property.VariantValue.AsDouble();
				break;

			default:
				new Exception("UI Node Type Null / Invalid / Not implemented when Initialising Values");
				break;
		}
	}

	private void _DisplayCentralShiftIndicatorToggle(bool toggle)
	{
		Config.PersistentStorage.DisplayCentralShiftIndicator.Value = toggle;
	}

	private void _DisplayRevGaugeShiftIndicatorToggle(bool toggle)
	{
		Config.PersistentStorage.DisplayRevGaugeShiftIndicator.Value = toggle;
	}

	private void _DisplayRevGaugePowerbandZoneToggle(bool toggle)
	{
		Config.PersistentStorage.DisplayRevGaugePowerbandZone.Value = toggle;
	}

	private void _DisplayRevGaugeEnginePowerInfo(Enums.PowerCurveOverlayType type)
	{
		Config.PersistentStorage.DisplayRevGaugeEnginePowerInfo.Value = type;
	}

	private void _PowerUnit(Enums.PowerUnitType type)
	{
		Config.PersistentStorage.PowerUnit.Value = type;
	}

	private void _TorqueUnit(Enums.TorqueUnitType type)
	{
		Config.PersistentStorage.TorqueUnit.Value = type;
	}

	private void _DataOutIPAddressValue(string address)
	{
		Config.PersistentStorage.DataOutIPAddress.Value = address;
	}
	private void _DataOutIPPortValue(double value)
	{
		Config.PersistentStorage.DataOutIPPort.Value = (int)Math.Round(value);
	}

	private void _HUDSafeFrameHorizontalValue(double value)
	{
		Config.PersistentStorage.HUDSafeFrameHorizontal.Value = value;
	}

	private void _HUDSafeFrameVerticalValue(double value)
	{
		Config.PersistentStorage.HUDSafeFrameVertical.Value = value;
	}

	private void _OverlayControlButtonToggle(bool toggle)
	{
		if (toggle)
		{
			if (_overlayPID != -1 && OS.IsProcessRunning(_overlayPID))
			{
				return;
			}

			_overlayPID = OS.CreateInstance([@"res://Scenes/OverlayUI.tscn", "--", "--overlay", $"--parent-pid={System.Environment.ProcessId}"]);

			OverlayProcess = Process.GetProcessById(_overlayPID);
			OverlayProcess.EnableRaisingEvents = true;

			OverlayProcess.Exited += (_, _) =>
			{
				Callable.From(() => enableToggleButton.ButtonPressed = false).CallDeferred();
			};

		}
		else
		{
			if (_overlayPID != -1)
			{
				OS.Kill(_overlayPID);
				_overlayPID = -1;
			}
		}
	}

	private int _overlayPID = -1;
}
