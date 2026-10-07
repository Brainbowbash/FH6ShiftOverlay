// SPDX-License-Identifier: GPL-3.0-or-later
// SPDX-FileCopyrightText: 2026 Brainbowbash
using Godot;
using System;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

namespace WindowHandler;

public partial class OverlayWindowHandler : Node
{
	private static Window _overlayWindow;

	private static bool _enabled = false;
	private static bool _focused = false;
	private static bool _windowShown = false;

	private static IntPtr _overlayHwnd = IntPtr.Zero;
	private static readonly (string Title, string ProcessName) WindowMatch = ("Forza Horizon 6", "forzahorizon6");

	private const int GWL_EXSTYLE = -20;
	private const int WS_EX_TRANSPARENT = 0x00000020;
	private const int WS_EX_LAYERED = 0x00080000;
	private const int WS_EX_TOOLWINDOW = 0x00000080;
	private const int WS_EX_APPWINDOW = 0x00040000;
	private const int SW_HIDE = 0;
	private const int SW_SHOW = 5;

	static private IntPtr _previousWindow = IntPtr.Zero;


	static public event Action OverlayActivated;

	public override void _Ready()
	{
		if (OS.GetCmdlineUserArgs().Contains("--overlay"))
		{
			_enabled = false;
			_overlayWindow = GetWindow();

			_overlayWindow.Borderless = false;

			_overlayWindow.Title = "Shift Overlay";

			_overlayWindow.Mode = Window.ModeEnum.Windowed;

			_overlayWindow.Size = new Vector2I(400, 300);

			_overlayWindow.ContentScaleSize = new Vector2I(800, 600);
			_overlayWindow.ContentScaleAspect = Window.ContentScaleAspectEnum.Expand;
			_overlayWindow.ContentScaleMode = Window.ContentScaleModeEnum.CanvasItems;
		}
		else
		{
			QueueFree(); // Remove OverlayWindowHandler autoload because uneeded for main menu
		}
	}


	public override void _Process(double delta)
	{
		bool focused = Focused;

		if (!_enabled && focused)
		{
			SetOverlayWindowProperties();
			_enabled = true;
			OverlayActivated();
		}
		else
		{
			if (focused)
			{
				ShowWindow();

				(Vector2I Position, Vector2I Size) targetPos = GetWindowPositionDetails();

				if (_overlayWindow.Mode != Window.ModeEnum.Windowed)
				{
					_overlayWindow.Mode = Window.ModeEnum.Windowed;
				}

				_overlayWindow.Size = targetPos.Size + new Vector2I(0, Math.Min(1, targetPos.Size.Y / 1080));
				_overlayWindow.Position = targetPos.Position;


			}
			else
			{
				HideWindow();
			}
		}

	}

	private static bool Focused
	{
		get
		{
			_focused = FocusedOnForza(_focused);
			return _focused;
		}
	}

	private static void ShowWindow()
	{
		if (!_windowShown)
		{
			_windowShown = true;
			_overlayWindow.Show();

			_overlayHwnd = new IntPtr(DisplayServer.WindowGetNativeHandle(DisplayServer.HandleType.WindowHandle, _overlayWindow.GetWindowId()));

			long exStyle = GetWindowLongPtr(_overlayHwnd, GWL_EXSTYLE).ToInt64();

			ShowWindow(_overlayHwnd, SW_SHOW);

			SetWindowLongPtr(_overlayHwnd, GWL_EXSTYLE, new IntPtr((exStyle | WS_EX_TRANSPARENT | WS_EX_LAYERED | WS_EX_TOOLWINDOW) & ~WS_EX_APPWINDOW)); // 
		}
	}

	private static void HideWindow()
	{
		if (_windowShown)
		{
			_windowShown = false;

			_overlayHwnd = new IntPtr(DisplayServer.WindowGetNativeHandle(DisplayServer.HandleType.WindowHandle, _overlayWindow.GetWindowId()));

			ShowWindow(_overlayHwnd, SW_HIDE);
		}
	}

	private static bool FocusedOnForza(bool focused)
	{
		IntPtr activeWindow = GetForegroundWindow();
		if (activeWindow == _previousWindow)
		{
			return focused;
		}

        _previousWindow = activeWindow;

        if (activeWindow == IntPtr.Zero) return false;

		StringBuilder titleBuilder = new StringBuilder(1024);
		GetWindowText(activeWindow, titleBuilder, titleBuilder.Capacity);

        if (titleBuilder.ToString() == WindowMatch.Title)
		{
			string windowProcess = "";
			GetWindowThreadProcessId(activeWindow, out uint processID);

			try
			{
				using Process process = Process.GetProcessById((int)processID);
				windowProcess = process.ProcessName;
			}
			catch
			{
				return false;
			}
			return windowProcess.ToLower() == WindowMatch.ProcessName;
		}
		return false;
	}

	private static void SetOverlayWindowProperties()
	{
		_overlayWindow.Unresizable = true;
		_overlayWindow.Unfocusable = true;
		_overlayWindow.AlwaysOnTop = true;

		_overlayWindow.Borderless = true;

		_overlayWindow.ContentScaleSize = new Vector2I(1920, 1081);
	}

	private static (Vector2I Position, Vector2I Size) GetWindowPositionDetails()
	{
		IntPtr activeWindow = GetForegroundWindow();

		if (GetClientRect(activeWindow, out WindowRect windowRect))
		{
			WindowPoint topLeftPoint = new WindowPoint { X = 0, Y = 0 };
			ClientToScreen(activeWindow, ref topLeftPoint);

			return (topLeftPoint.Point, windowRect.Size);
		}
		return (Vector2I.Zero, Vector2I.Zero);
	}

	[DllImport("user32.dll")]
	private static extern IntPtr GetForegroundWindow();

	[DllImport("user32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
	private static extern int GetWindowText(IntPtr hWnd, StringBuilder lpString, int nMaxCount);

	[DllImport("user32.dll")]
	private static extern uint GetWindowThreadProcessId(IntPtr hWnd, out uint lpdwProcessId);

	[DllImport("user32.dll")]
	private static extern bool GetClientRect(IntPtr hWnd, out WindowRect rect);

	[DllImport("user32.dll")]
	private static extern bool ClientToScreen(IntPtr hWnd, ref WindowPoint point);

	[DllImport("user32.dll", EntryPoint = "GetWindowLongPtr")]
	private static extern IntPtr GetWindowLongPtr(IntPtr hWnd, int nIndex);

	[DllImport("user32.dll", EntryPoint = "SetWindowLongPtr")]
	private static extern IntPtr SetWindowLongPtr(IntPtr hWnd, int nIndex, IntPtr dwNewLong);
	[DllImport("user32.dll")]
	private static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

	[DllImport("user32.dll", SetLastError = true)]
	private static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int X, int Y, int cx, int cy, uint uFlags);

	[StructLayout(LayoutKind.Sequential)]
	public struct WindowRect
	{
		public int Left;
		public int Top;
		public int Right;
		public int Bottom;

		public Vector2I Size
		{
			get { return new Vector2I(Right - Left, Bottom - Top); }
		}
	}
	[StructLayout(LayoutKind.Sequential)]
	public struct WindowPoint
	{
		public int X;
		public int Y;
		public Vector2I Point
		{
			get { return new Vector2I(X, Y); }
		}
	}
}
