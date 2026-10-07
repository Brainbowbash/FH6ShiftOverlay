// SPDX-License-Identifier: GPL-3.0-or-later
// SPDX-FileCopyrightText: 2026 Brainbowbash
using Godot;
using System;
namespace OverlayUI;
public partial class PowerbandZone : Control
{
	private Line2D arcLine;
	private ShaderMaterial arcLineShader;

	private bool shown;

	public override void _Ready()
	{
		arcLine = (Line2D)FindChild("ArcLine");
		arcLineShader = (ShaderMaterial)arcLine.Material;
		CreateArc(150f,64);
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

	private void CreateArc(float radius, int pointCount)
	{
		arcLine.ClearPoints();
		for (int i = 0; i < pointCount; i++)
		{
			(float sin, float cos) = MathF.SinCos(Mathf.Lerp(Mathf.DegToRad(-241f), 0f, Mathf.Remap(i, 0, pointCount-1, 0f,1f)));
			arcLine.AddPoint(new Vector2(cos * radius, sin * radius));
		}
	}

	public void DisplayArc(float lower, float upper, bool certainty)
	{
		arcLineShader.SetShaderParameter(@"limits", new Vector2 (lower,upper));
		arcLineShader.SetShaderParameter(@"limit_sharpness", new Vector2(300f, certainty ? 300f : 20f));
	}
}
