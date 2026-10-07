// SPDX-License-Identifier: GPL-3.0-or-later
// SPDX-FileCopyrightText: 2026 Brainbowbash
using Godot;
using System;
namespace OverlayUI;
public partial class FullPowerDisplay : Control
{
	private Enums.PowerUnitType powerUnitType;
	private Enums.TorqueUnitType torqueUnitType;

	private ColorRect BigGrid;
	private ShaderMaterial BigGridShader;

	private Line2D PowerLine;
	private Line2D TorqueLine;

	private Label PeakPower;
	private Label PeakTorque;

	private Label CurrentPower;
	private Label CurrentTorque;

	private bool shown;

	public override void _Ready()
	{
		BigGrid = (ColorRect)FindChild("BigGrid", true);
		BigGridShader = (ShaderMaterial)BigGrid.Material;

		PowerLine = (Line2D)FindChild("PowerLine", true);
		TorqueLine = (Line2D)FindChild("TorqueLine", true);

		PeakPower = (Label)FindChild("PeakPower", true);
		PeakTorque = (Label)FindChild("PeakTorque", true);

		CurrentPower = (Label)FindChild("CurrentPower", true);
		CurrentTorque = (Label)FindChild("CurrentTorque", true);

		powerUnitType = 0;
		torqueUnitType = 0;
		Display(false);
	}

	public void Display(bool show)
	{
		shown = show;
		if (show)
		{
			Show();
		}
		else
		{
			Hide();
		}
	}

	public void SetUnits(Enums.PowerUnitType PowerUnit, Enums.TorqueUnitType TorqueUnit)
	{
		powerUnitType = PowerUnit;
		torqueUnitType = TorqueUnit;
	}

	private readonly float[] scaleChange = [2f, 2.5f, 2f]; // 1 -> 2 -> 5 -> 10 -> etc
	public void UpdateCurve((float NormalisedRPM, float Power, float Torque)[] values, float maxPower, float maxTorque, float maxGaugeRPM)
	{
		if (!shown) return;

		float maxReadingY = MathF.Max(maxPower, maxTorque);

		float yDivisionSize = 1f;
		int scaleIncreases = 0;

		while (maxReadingY / yDivisionSize > 10f)
		{
			yDivisionSize *= scaleChange[scaleIncreases++ % 3];
		}

		float xSubdivisions = MathF.Round(maxGaugeRPM / 1000f);
		float ySubdivisions = MathF.Ceiling(maxReadingY / yDivisionSize);

		float maxY = ySubdivisions * yDivisionSize;

		BigGridShader.SetShaderParameter(@"subdivisions", new Vector2(xSubdivisions, ySubdivisions));

		TorqueLine.ClearPoints();
		PowerLine.ClearPoints();

		if (maxY == 0)
		{
			maxY = 1;
		}

		for (int i = 0; i < values.Length; i++)
		{
			float x = values[i].NormalisedRPM * 144.0f;
			float yPower = Mathf.Clamp(values[i].Power / maxY * -154.0f, -154.0f, 0f);
			float yTorque = Mathf.Clamp(values[i].Torque / maxY * -154.0f, -154.0f, 0f);

			TorqueLine.AddPoint(new Vector2 { X = x, Y = yTorque });
			PowerLine.AddPoint(new Vector2 { X = x, Y = yPower });
		}
	}

	public void UpdatePeakValues((float Value, float RPM) peakPower, (float Value, float RPM) peakTorque)
	{
		if (!shown) return;

		PeakPower.Text = $"{MathF.Floor(peakPower.Value):F0} {Enums.UnitNames.Power[(int)powerUnitType]} @ {MathF.Round(peakPower.RPM / 100f) / 10f:F1}k";
		PeakTorque.Text = $"{MathF.Floor(peakTorque.Value):F0} {Enums.UnitNames.Torque[(int)torqueUnitType]} @ {MathF.Round(peakTorque.RPM / 100f) / 10f:F1}k";
	}

	public void UpdateCurrentValues(float power, float torque)
	{
		if (!shown) return;

		CurrentPower.Text = $"{MathF.Floor(power):F0} {Enums.UnitNames.Power[(int)powerUnitType]}";
		CurrentTorque.Text = $"{MathF.Floor(torque):F0} {Enums.UnitNames.Torque[(int)torqueUnitType]}";
	}

}
