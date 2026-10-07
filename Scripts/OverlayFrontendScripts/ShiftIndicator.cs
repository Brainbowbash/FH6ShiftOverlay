// SPDX-License-Identifier: GPL-3.0-or-later
// SPDX-FileCopyrightText: 2026 Brainbowbash
using Godot;
namespace OverlayUI;
public partial class ShiftIndicator : Control
{
	private Polygon2D shiftUp;
	private Polygon2D shiftDown;

	private bool shown;
	public override void _Ready()
	{
		shiftUp = (Polygon2D)FindChild("ShiftUpArrow", true);
		shiftDown = (Polygon2D)FindChild("ShiftDownArrow", true);
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
	public void UpdateShiftArrow(int direction)
	{
		if (!shown) return;

		switch (direction)
		{
			case 1:
				shiftDown.Hide();
				shiftUp.Show();
				break;

			case -1:
				shiftUp.Hide();
				shiftDown.Show();
				break;

			case 0:
			default:
				shiftDown.Hide();
				shiftUp.Hide();
				break;
		}
	}
}
