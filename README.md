# DiceVaders Ultrawide

Created and maintained by **NorthernDale** (**moistman7** on GitHub). This is my source repository for [my Nexus Mods release](https://www.nexusmods.com/dicevaders/mods/1).

A BepInEx 6 IL2CPP plugin for height-based ultrawide canvas scaling, adjustable encounter HUD/board sizes, and optional startup resolution selection. HUD and board default to **100%**.

## Install

1. Install BepInEx 6 **Windows x64 IL2CPP**. The local installation uses bleeding-edge build 788. Launch once to generate interop assemblies, then exit.
2. Extract the plugin release ZIP into the DiceVaders folder. The DLL belongs at `BepInEx/plugins/DiceVadersUltrawide.dll`.
3. Launch the game. Existing settings are preserved. Restart after upgrading the plugin.

[BepInEx downloads](https://builds.bepinex.dev/projects/bepinex_be). Releases contain this plugin only, without BepInEx, Unity, generated interop DLLs, or game assets. Remove the plugin DLL to uninstall.

## Menu and controls

**F10** opens the mod panel directly from gameplay, Pause, or Settings. Settings and Pause also have an **ULTRAWIDE MOD** entry. Opening from Pause does **not** open the game's Options screen. Closing returns to the screen underneath.

The panel has separate HUD and board sliders (100-135%), an enable button, Reset to 100%, and Close. Escape closes it. Changes save immediately. The overlay saves/restores the Unity clock, cursor, enabled game input handlers, and EventSystems. Real-time game coroutines may continue; full pause behavior and controller navigation still need testing.

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

The mod panel's **HUD FRAME** button toggles between **FULL WIDTH** and **CENTERED 16:9**. Centered mode brings encounter HUD panels into a height-based 16:9 area while keeping the game ultrawide and board size independent. It works at 100% HUD size too. The setting saves as `Interface/CenterHud16By9` (default false); Reset to 100% changes sizes only.

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

Version 0.6.14 adds the centered 16:9 HUD frame, 100% default sizes, persistent sizing, and mod menu click fixes. Set `DICEVADERS_UW_PROFILE=1` to log frame timing every 10 seconds. Opt-in runtime checks at 3440x1440 pass at 100%, 123% and 135% HUD size: board unchanged by the frame, repeated sizing stable, simulated panel animation never overwritten, and full width restored exactly. Measured mod CPU time dropped from 2.6-3.2 ms to about 0.3 ms per frame. Centered and full-width layouts were checked visually in an encounter; full combat actions remain unverified.

Version 0.6.13 fixes encounter pointer sizing: mouse hover/click/release/cancel handling temporarily uses the same enlargement and screen-edge positioning as rendering. Nested input calls do not multiply the scale again, and temporary transforms are restored on exit. Release build passes. Live checks at 3440x1440 confirmed correct pointer highlights/tooltips on enlarged outer edges at 123% and 135%, plus center and top tiles at 135%. Full combat actions remain unverified.

Version 0.6.12 includes the approved Settings/Pause panels, sharper pause text, a more visible blue Options entry with a cyan border, and a centred language selector with full-screen dimming. Main-menu icon spacing stays at the game's default.

The menus use protected copies of the game's contextual fonts, with Arial as a fallback. Settings and Pause opening hooks prepare their entries immediately; Options exit hooks hide the entry during transitions. Button hit targets use standard Unity Images, while angled artwork is rendered as geometry.

The mod reuses corner buffers, skips hidden-entry layout work, caches the main-menu controller and available handwriting font, and updates panel text only when values change. These reduce unnecessary work; frametime improvements have not been benchmarked.

The Release build passes with zero warnings/errors against BepInEx 6 build 788 and Unity 6000.3.10f1 interop assemblies. Runtime checks passed for Toggle, Reset, and Close raycast targets and pointer callbacks, plus language-panel centring and backdrop coverage at 3440x1440. The author confirmed the menu appearance in game. Controller navigation and overlap at every size/aspect ratio remain unverified.

Developer checks use process environment variables: `DICEVADERS_UW_MENU_TEST=1` opens/closes the panel; add `DICEVADERS_UW_BUTTON_TEST=1` to check button callbacks and restore saved values. `DICEVADERS_UW_LAYOUT_TEST=1` advances from the startup prompt, opens the language selector, and logs its geometry. An optional `DICEVADERS_UW_MENU_CAPTURE` PNG path requests an end-of-frame capture. Hidden-window captures can be black and are not proof of visual correctness. Logs are in `BepInEx/LogOutput.log`.

## Credits

Mod author and maintainer: **NorthernDale / moistman7**. This is the author's own repository.

BepInEx, Il2CppInterop, Harmony, and Unity belong to their respective authors. Dependencies are referenced locally; game assets are not redistributed.

