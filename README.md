# FH6ShiftOverlay
<p align="center">
  <picture>
    <source width = 256 media="(prefers-color-scheme: dark)" srcset="https://github.com/Brainbowbash/FH6ShiftOverlay/raw/main/Icons/shiftdiallogo_white_512.png" />
    <source width = 256 media="(prefers-color-scheme: light)" srcset="https://github.com/Brainbowbash/FH6ShiftOverlay/raw/main/Icons/shiftdiallogo_black_512.png" />
    <img width = 256 src="https://github.com/FH6ShiftOverlay/FH6ShiftOverlay/raw/main/Icons/shiftdiallogo_white_512.png" alt="FH6ShiftOverlay Logo" />
  </picture>
</p>

## Informative Overlay for Forza Horizon 6
**FH6ShiftOverlay** is an informative, minimalist overlay for the open-world racing game **Forza Horizon 6**, featuring essential shift-point information catered to each car.
It provides an assortment of toggleable overlay options to choose from, customising the overlay to display only what you need.

## How it works
**FH6ShiftOverlay** uses the ["Data Out"](https://support.forza.net/hc/en-us/articles/51744149102611-Forza-Horizon-6-Data-Out-Documentation) feature available in Forza Horizon 6 to receive telemetry data about the car.
This data is processed, and mapped to build an approximation of the vehicle's engine power curve and gear ratios.
The approximated engine power curve and relation between the gear ratios are used to maximise for optimal shift-points for each gear using numerical methods.
Shift-points are re-generated when switching to a different car (or has been just upgraded) and cannot be fully displayed until a suitable region of the engine power curve has been mapped,
in addition to the car being driven in all gears.

## Overlay Visuals
**FH6ShiftOverlay** features several, simplistic UI elements. Every overlay UI element is optional and can be disabled individually.
Provided with the correct *HUD Safe Frame* values, the overlay will adjust to align with the in-game HUD.

> The image below features multiple of the UI elements included in the overlay:
> - Central Shift Arrows
> - Gauge Shift Arrows
> - Gauge Powerband Indicator (only compatible with
> - Engine Power and Torque Info (Full details)
<img width="1920" height="1080" alt="FH6ShiftOverlay " src="https://github.com/user-attachments/assets/1bb97a59-829f-4a5d-818d-2b09a9d4ef60" />

## Compatibility
**FH6ShiftOverlay** is natively compatible with Windows operating systems only: the Windows API is required to determine the state of the Forza Horizon 6 window and is used to appropriately resize the overlay's window.

As the overlay will dynamically adjust to the FH6 window, it is compatible with any resolution and any refresh rate, automatically adapting to the contents of the game.

## Installation
To install the overlay, download the overlay from the [Releases](releases) page and extract the .exe file from the downloaded .zip file.
> [!WARNING]
> When running the application, Windows Defender SmartScreen may trigger, as the program is not signed.
> 
> If this occurs, select More info -> Run anyway.
> 
> If you are unsure about the safety of the program, you are welcome to check the source code yourself or build it.

Start up **Forza Horizon 6** and enable the **Data Out** features in Settings -> HUD & Gameplay -> Telemetry.

> [!TIP]
> ### Recommended Data Out settings
> It is recommended to use the following in-game settings when using this overlay:
> 
> **Data Out IP Address:** 127.0.0.1
> 
> **Data Out IP Port:** 8000

Upon the running of the program, you will be met with a configuration menu.

Scroll to the **Data Out** section of the menu and set **Data Out IP Address** and **Data Out IP Port** to match the same values as the ones in-game.

> [!NOTE]
> ### HUD Safe Frame
> If you have changed your HUD Safe Frame settings from the default in-game, make sure you adjust the HUD Safe Frame settings in the configuration menu to match.
>
> If the HUD Safe Frame settings are the default, keep the **HUD Safe Frame Horizontal** to **5.2** and **HUD Safe Frame Vertical** to **9.3**.
> 
> The game only displays the integer value, it does not display the hidden decimal value. Changing the in-game value will always make it an integer though.

Adjust your overlay preferences in the **Overlay Display** section of the configuration menu.
Once preferences have been configured, press the **Enable** button in the menu to enable the overlay.

The overlay window should pop up. You can then focus the game window by clicking on it a few times and the overlay window should resize to accommodate the full size of the game window.
> [!TIP]
> ### Capturing the Overlay with Recording Software
> If you choose to capture the overlay with recording software such as **OBS**, steps must be taken to ensure the capture of the overlay.
> Steps below are for **OBS**:
>
> Enable the overlay from the configuration menu, but don't focus onto the game yet.
> 
> Create a new **Game Capture** Source with
> - **Mode -> Capture specific window**
> - **Window -> \[FH6ShiftOverlay_vX.X.X.exe]: Shift Overlay**
> - **Window Match Priority -> Window title must match**
> - **Allow Transparency -> Enabled**
> 
> If **OBS** is having difficulty hooking onto the overlay window, try continuously resizing the overlay window until **OBS** has hooked on.
>
> Then go in-game, the overlay window should be visible on **OBS**.

Finally, you should check that the overlay updates as you drive. If it doesn't update, check that the **Data Out IP Address** and **Data Out IP Port** values match across the configuration menu and the game.

## Usage
When in-game with the overlay window active, the overlay should be visible and should update from the data received from Forza.

The data is accumulated over time to build an approximation of the current car's gears and engine power/torque curves
> [!NOTE]
> The overlay is reliant on the data that the game provides, and there are a few measures that can be done to ensure the accuracy of the overlay:
>
> Ensure that a majority of the engine curve is mapped out. To do this, start at a low engine RPM in a moderate gear and full-throttle until redline. Make sure the car doesn't slip.
> This will provide the overlay with data about the region of the engine curve and update the curve with power/torque values when the car is under boost (where applicable).
>
> Shift through all the gears in the car. The overlay doesn't know about gears that you haven't shifted to, and will thus be unable to provide shift-points for particular gears, defaulting to the highest RPM.

## Build Instructions
This project was made in the [Godot](https://github.com/godotengine/godot) game engine version 4.7.2.stable.mono using .NET 8.0.

1. Download and unzip the source code
2. Download or build [Godot 4.7.2 .NET version or above](https://godotengine.org/download/archive/4.7.2-stable/)
3. Download and install [.NET 8.0 or above](https://dotnet.microsoft.com/en-us/download/dotnet/8.0)
4. Open the Godot game engine and import the project
5. Build the project once with Alt+B or press the Build button.
6. Select Project -> Export... -> Add... -> Windows Desktop
7. If there is **No export template found**, press **Manage Export Templates** and download the export template for the platform.
8. Go back to export menu and press **Export Project...**

> [!TIP]
> Enable **Dotnet -> Include Scripts Content** and **Binary Format -> Embed PCK** in the **Export** menu in Godot.
> This will allow the executable to be moved outside of the initial directory without errors.

> [!NOTE]
> text
> 
