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

0.4.0 preserves the original UI proportions and removes the experimental panel enlargement from 0.3.0. It is a prerelease. It compiles against the installed BepInEx 6 build 788 and Unity 6000.3.10f1 interop assemblies with zero warnings/errors. Runtime validation of this revision is pending. The original screenshots show 3440x1440 gameplay; they are not evidence that this revision fixes tutorial positioning or bottom-panel clipping. This plugin changes canvas scaling, not individual UI anchors, game artwork, camera framing, or render textures.

Before treating this as stable, check F8 restoration, tutorial placement, the bottom invasion panel, scene transitions, 16:9, 3440x1440, and resolution changes. F9 diagnostics can help investigate any remaining issues.

## Credits and permissions

Original mod and source: NorthernDale. BepInEx and Il2CppInterop belong to their respective authors. Third-party components are referenced locally and are not redistributed. No additional source redistribution license is granted by this repository; contact the author for reuse permission.

## Original UI proportions (0.4.0)

The UI uses height-based canvas scaling (`General/CanvasMatch = 1`) so its elements retain their original proportions and height-relative size at ultrawide resolutions. Existing game anchors lay out panels across the wider canvas. This revision removes the 0.3.0 HUD/board enlargement, transform offsets, and F6/F7 shortcuts. Old Interface/HudScale and BoardScale settings are ignored. F8 still toggles the ultrawide canvas fix.

This restores the original scaling approach; it does not stretch the 16:9 image or enlarge individual panels. The wider layout naturally has more horizontal space. Exact panel placement and tutorial screens still require runtime verification.
