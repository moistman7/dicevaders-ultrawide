# DiceVaders Ultrawide

Created and maintained by **NorthernDale** (**moistman7** on GitHub). This is my source repository for [my Nexus Mods release](https://www.nexusmods.com/dicevaders/mods/1).

A BepInEx 6 IL2CPP plugin for height-based ultrawide canvas scaling, adjustable encounter HUD/board sizes, and optional startup resolution selection. HUD and board default to **123%**.

## Install

1. Install BepInEx 6 **Windows x64 IL2CPP**. The local installation uses bleeding-edge build 788. Launch once to generate interop assemblies, then exit.
2. Extract the plugin release ZIP into the DiceVaders folder. The DLL belongs at `BepInEx/plugins/DiceVadersUltrawide.dll`.
3. Launch the game. Existing settings are preserved. Restart after upgrading the plugin.

[BepInEx downloads](https://builds.bepinex.dev/projects/bepinex_be). Releases contain this plugin only, without BepInEx, Unity, generated interop DLLs, or game assets. Remove the plugin DLL to uninstall.

## Menu and controls

**F10** opens the mod panel directly from gameplay, Pause, or Settings. Settings and Pause also have an **ULTRAWIDE MOD** entry. Opening from Pause does **not** open the game's Options screen. Closing returns to the screen underneath.

The panel has separate HUD and board sliders (100-135%), an enable button, Reset to 123%, and Close. Escape closes it. Changes save immediately. The overlay saves/restores the Unity clock, cursor, enabled game input handlers, and EventSystems. Real-time game coroutines may continue; full pause behavior and controller navigation still need testing.

| Default key | Action |
| --- | --- |
| F6 | Decrease HUD and board size by 2 percentage points |
| F7 | Increase HUD and board size by 2 percentage points |
| F8 | Toggle the fix and enlargement |
| F9 | Write canvas/camera diagnostics |
| F10 | Open/close the mod panel |

All five bindings are configurable. Close the game and edit `BepInEx/config/local.dicevaders.ultrawide.cfg`:

```ini
[Hotkeys]
DecreaseSize = F6
IncreaseSize = F7
ToggleFix = F8
Diagnostics = F9
Menu = F10
```

Use Input System key names such as F12, PageUp, PageDown, Home, or End. `None` disables a shortcut. Assign different keys to different actions. Bindings are single keys, without modifier combinations. Restart to apply changes. Rebinding is through the config file, not a menu capture control.

## Configuration

Edit the config with the game closed.

- `General/Enabled`: enable the fix (default true).
- `General/CanvasMatch`: 0-1; default 1 matches height, retaining proportions. Lower values zoom the canvas.
- `Interface/HudScale` and `BoardScale`: 1.00-1.35; default 1.23. Enlargement applies above 16:9. Set both to 1 for original sizes.
- `Resolution/ForceResolution`: default true. Disable to use game settings alone.
- `Width` / `Height`: default 0; each automatic dimension uses the main display size independently.
- `ScreenMode`: FullScreenWindow (default), ExclusiveFullScreen, MaximizedWindow, or Windowed.
- `DelaySeconds` / `RetrySeconds`: default 2 / 15, range 0-120. Retries run once per second during the startup window, then leave resolution alone.

Disabling the fix restores tracked canvas/enlargement changes and stops resolution retries. It does not revert an already selected resolution. Canvas discovery runs every 250 ms; a Harmony hook additionally applies the match before canvases calculate scaling to avoid the polling delay during transitions.

## Build

Requires a .NET SDK supporting net6.0 and a game installation with BepInEx plus generated interop assemblies:

```powershell
dotnet build -c Release -p:GameDir="D:/SteamLibrary/steamapps/common/DiceVaders"
```

Output: `bin/Release/net6.0/DiceVadersUltrawide.dll`. Building does not install it.

## Validation and limitations

Builds against BepInEx 6 build 788 and Unity 6000.3.10f1 interop assemblies with zero warnings/errors. The 0.5.7 menu uses fresh Unity UI text with its own Arial font, protected from unused-asset unloading. It never borrows a scene font. This addresses the destroyed-font error observed after scene changes, and avoids inheriting text animations, masking, or visibility state. Loaded native slider artwork is reused where available.

Opening the menu logs the HUD label's generated vertex count. For a developer startup smoke check, launch with `DICEVADERS_UW_MENU_TEST=1` in the process environment; it opens the panel after UI startup and closes it two seconds later, logging text geometry and clock restoration. This is not a visual screenshot test.

Panel placement, slider interaction, scene changes, repeated reopening, tutorial screens, and larger-size overlap still need visual/runtime verification. Bounds correction applies only to supported RectTransform holders, so larger sizes can crowd or clip artwork. Logs are in `BepInEx/LogOutput.log`.

## Credits

Mod author and maintainer: **NorthernDale / moistman7**. This is the author's own repository.

BepInEx, Il2CppInterop, Harmony, and Unity belong to their respective authors. Dependencies are referenced locally; game assets are not redistributed.

The pause and Settings controllers are discovered even while inactive. Entry visibility is checked every frame, and the overlay is prebuilt at startup, removing the previous one-second wait when opening an already-loaded pause screen.

Pause styling (0.5.8): when native Settings assets have not loaded, the panel uses locally generated angled shapes, blue filled sliders with slash handles, and slim teal buttons. The pause entry no longer borrows green Resume artwork. Native Settings assets remain in use when available. These generated textures contain no copied game artwork.
