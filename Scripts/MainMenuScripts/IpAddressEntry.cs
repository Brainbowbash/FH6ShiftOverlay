// SPDX-License-Identifier: GPL-3.0-or-later
// SPDX-FileCopyrightText: 2026 Brainbowbash
using Godot;

namespace MainMenu.IpAddressEntry;
public partial class IpAddressEntry : LineEdit
{
	private bool _isAlert = false;
	private bool isAlert
	{
		get { return _isAlert; }
		set
		{
			_isAlert = value;
			ThemeTypeVariation = _isAlert ? "LineEditAlert" : "LineEditNormal";
		}
	}
	private string oldText = "";

	private readonly RegEx partialIPAddressRegex = RegEx.CreateFromString(@"^((25[0-5]|2[0-4]\d|1\d\d|[1-9]\d|\d)\.){0,3}(25[0-5]|2[0-4]\d|1\d\d|[1-9]\d|\d)?$");
	private readonly RegEx completeIPAddressRegex = RegEx.CreateFromString(@"^((25[0-5]|2[0-4]\d|1\d\d|[1-9]\d|\d)\.){3}(25[0-5]|2[0-4]\d|1\d\d|[1-9]\d|\d)$");

	public override void _Ready()
	{
		TextChanged += FilterTextChanged;
		TextSubmitted += ValidateTextSubmission;
		FocusExited += ValidateTextSubmission;
	}

	private void FilterTextChanged(string newText)
	{
		if (partialIPAddressRegex.Search(newText) is null)
		{
			int newCaretColumn = CaretColumn - newText.Length + oldText.Length;
			Text = oldText;
			CaretColumn = newCaretColumn;
		}
		else
		{
			oldText = newText;
			if (completeIPAddressRegex.Search(newText) is not null)
			{
				isAlert = false;
			}
		}


	}

	private void ValidateTextSubmission()
	{
		ValidateTextSubmission(Text);
	}

	private void ValidateTextSubmission(string newText)
	{
		isAlert = completeIPAddressRegex.Search(newText) is null;
		if (!isAlert)
		{
			EmitSignal(SignalName.EntryCommitted, newText);
		}

	}

	[Signal]
	public delegate void EntryCommittedEventHandler(string value);

}
