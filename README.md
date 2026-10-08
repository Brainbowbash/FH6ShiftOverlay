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
**FH6ShiftOverlay** features several UI elements. Every overlay UI element is optional and can be disabled individually.
Provided with the correct *HUD Safe Frame* values, the overlay will adjust to align with the in-game HUD.
The overlay is compatible with any resolution and any refresh rate, automatically adjusting to the contents of the game.

The image below features multiple of the UI elements included in the overlay:
- Central Shift Arrows
- Gauge Shift Arrows
- Gauge Powerband Indicator
- Engine Power and Torque Info (Full details)
<img width="1920" height="1080" alt="FH6ShiftOverlay " src="https://github.com/user-attachments/assets/1bb97a59-829f-4a5d-818d-2b09a9d4ef60" />

