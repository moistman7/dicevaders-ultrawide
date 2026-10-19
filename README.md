# DiceVaders Ultrawide

BepInEx 6 IL2CPP plugin by NorthernDale. Adjusts Unity canvas scaling at wider-than-reference aspect ratios so the UI matches screen height. Includes optional startup resolution selection.

Original mod: https://www.nexusmods.com/dicevaders/mods/1

## Install

1. Install BepInEx 6 **Windows x64 IL2CPP**. The original installation uses bleeding-edge build 788. Launch the game once to generate interop assemblies, then exit.
2. Extract the release ZIP into the DiceVaders folder. The plugin belongs at `BepInEx/plugins/DiceVadersUltrawide.dll`.
3. Start the game. Existing settings are preserved.

BepInEx downloads and licensing: https://builds.bepinex.dev/projects/bepinex_be

The download contains this plugin only. It does not include BepInEx, Unity, generated interop assemblies, or game files. Remove the plugin DLL to uninstall.

## Controls and settings

- **F8** toggles the fix and saves the setting. Disabling also stops startup resolution retries; it does not revert a resolution already selected.
- **F9** writes camera and canvas diagnostics to `BepInEx/LogOutput.log`.
- Configuration: `BepInEx/config/local.dicevaders.ultrawide.cfg` (generated on first launch). Edit with the game closed.
- `CanvasMatch`: 0 to 1; default 1 matches height.
- `ForceResolution`: default true. Set false to use game settings alone.
- `Width` / `Height`: default 0; each automatic dimension independently uses the main display size.
- `ScreenMode`: FullScreenWindow (default), ExclusiveFullScreen, MaximizedWindow, or Windowed. Invalid values fall back to FullScreenWindow.
- `DelaySeconds` / `RetrySeconds`: default 2 / 15. Resolution retries run once per second during this startup window, then leave settings alone. Both values accept 0 to 120 seconds.

Canvas discovery runs every 250 ms and original scaling is restored when disabling the fix or returning to a narrower aspect ratio. New scene canvases may take up to 250 ms to adjust.

## Build

Requires a .NET SDK supporting net6.0 and a local game installation with BepInEx and generated interop assemblies:

```powershell
dotnet build -c Release -p:GameDir="D:/SteamLibrary/steamapps/common/DiceVaders"
```

Output: `bin/Release/net6.0/DiceVadersUltrawide.dll`. Building does not install or overwrite plugins.

## Validation and limitations

0.5.0 defaults to the user-selected 123% HUD and board sizes and adds a settings panel styled with native assets. It is a prerelease. It compiles against the installed BepInEx 6 build 788 and Unity 6000.3.10f1 interop assemblies with zero warnings/errors. Runtime validation of this revision is pending. The original screenshots show 3440x1440 gameplay; they are not evidence that this revision fixes tutorial positioning or bottom-panel clipping. This plugin changes canvas scaling, not individual UI anchors, game artwork, camera framing, or render textures.

Before treating this as stable, check F8 restoration, tutorial placement, the bottom invasion panel, scene transitions, 16:9, 3440x1440, and resolution changes. F9 diagnostics can help investigate any remaining issues.

## Credits and permissions

Original mod and source: NorthernDale. BepInEx and Il2CppInterop belong to their respective authors. Third-party components are referenced locally and are not redistributed. No additional source redistribution license is granted by this repository; contact the author for reuse permission.

## Optional UI sizing (0.5.0)

Default HUD and board size is 123%, matching the selected gameplay appearance. F7 increases both by 2 percentage points; F6 decreases both, down to 100%. Size changes are saved. The maximum is 135%. F8 disables the fix and enlargement together.

Set `Interface/HudScale` and `Interface/BoardScale` independently in the configuration (1.00 = 100%). These settings apply only above 16:9. Existing size preferences are retained on upgrades. For the original appearance set both to 1.

Enlargement remains experimental. Screen-edge correction applies only to supported RectTransform holders; larger sizes can crowd panels or clip artwork. Visual placement, animation, and input still need in-game testing.

## Mod menu

Open the game's Settings screen, then click **ULTRAWIDE MOD** at the bottom, or press **F10** while Settings is visible. The panel uses the game's actual slider artwork and font, with separate HUD and board sliders (100–135%), an ultrawide enable button, **Reset to 123%**, and Close. Changes save immediately; no Apply action is required. Escape closes the panel (the game's own Escape handler may also exit Settings).

The menu exists only while Settings is visible and does not introduce a second pause system. Existing F6/F7 and F8 shortcuts remain available. Menu pointer interactions, navigation, and visual layout require in-game testing; controller navigation is not yet verified. Native controls are cloned with fresh events, and game scripts on slider clones are disabled so they cannot change audio settings.
