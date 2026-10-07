// SPDX-License-Identifier: GPL-3.0-or-later
// SPDX-FileCopyrightText: 2026 Brainbowbash
using Godot;
using System;
namespace OverlayUI;
public partial class SimplePowerDisplay : Control
{
	private Enums.PowerUnitType powerUnitType;

	private ColorRect SmallGrid;
	private ShaderMaterial SmallGridShader;
	private Line2D PowerLine;
	private Label CurrentPower;

	private bool shown;

	public override void _Ready()
	{
		SmallGrid = (ColorRect)FindChild("SmallGrid", true);
		SmallGridShader = (ShaderMaterial)SmallGrid.Material;

		PowerLine = (Line2D)FindChild("PowerLine", true);

		CurrentPower = (Label)FindChild("CurrentPower", true);

		powerUnitType = 0;
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

	public void SetUnits(Enums.PowerUnitType PowerUnit)
	{
		powerUnitType = PowerUnit;
	}

	private readonly float[] scaleChange = [2f, 2.5f, 2f]; // 1 -> 2 -> 5 -> 10 -> etc
	public void UpdateCurve((float NormalisedRPM, float Power, float Torque)[] values, float maxPower, float maxGaugeRPM)
	{
		if (!shown) return;

		float maxReadingY = maxPower;

		float yDivisionSize = 1f;
		int scaleIncreases = 0;

		while (maxReadingY / yDivisionSize > 6f)
		{
			yDivisionSize *= scaleChange[scaleIncreases++ % 3];
		}

		float xSubdivisions = MathF.Round(maxGaugeRPM / 1000f);
		float ySubdivisions = MathF.Ceiling(maxReadingY / yDivisionSize);

		float maxY = ySubdivisions * yDivisionSize;

		SmallGridShader.SetShaderParameter(@"subdivisions", new Vector2(xSubdivisions, ySubdivisions));

		PowerLine.ClearPoints();

		if (maxY == 0)
		{
			maxY = 1;
		}

		for (int i = 0; i < values.Length; i++)
		{
			float x = values[i].NormalisedRPM * 144.0f;
			float yPower = Mathf.Clamp(values[i].Power / maxY * -36.0f, -36.0f, 0f);

			PowerLine.AddPoint(new Vector2 { X = x, Y = yPower });
		}
	}

	public void UpdateCurrentValues(float power)
	{
		if (!shown) return;

		CurrentPower.Text = $"{MathF.Floor(power):F0} {Enums.UnitNames.Power[(int)powerUnitType]}";
	}

}
