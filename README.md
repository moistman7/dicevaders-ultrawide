# DiceVaders Ultrawide

Created and maintained by **NorthernDale** (**moistman7** on GitHub). This is my source repository for the DiceVaders Ultrawide mod published on Nexus Mods.

BepInEx 6 IL2CPP plugin. Adjusts Unity canvas scaling at wider-than-reference aspect ratios so the UI matches screen height. Includes optional startup resolution selection.

My Nexus Mods page: https://www.nexusmods.com/dicevaders/mods/1

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

Canvas discovery runs every 250 ms and original scaling is restored when disabling the fix or returning to a narrower aspect ratio. A CanvasScaler hook also applies scaling before new canvases calculate their layout.

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

Mod author and maintainer: **NorthernDale** (GitHub: [moistman7](https://github.com/moistman7)). This is the author's own repository, not a third-party adaptation.

BepInEx, Il2CppInterop, Harmony, and Unity belong to their respective authors. The plugin references these components locally and does not redistribute game assets.

## Optional UI sizing (0.5.0)

Default HUD and board size is 123%, matching the selected gameplay appearance. F7 increases both by 2 percentage points; F6 decreases both, down to 100%. Size changes are saved. The maximum is 135%. F8 disables the fix and enlargement together.

Set `Interface/HudScale` and `Interface/BoardScale` independently in the configuration (1.00 = 100%). These settings apply only above 16:9. Existing size preferences are retained on upgrades. For the original appearance set both to 1.

Enlargement remains experimental. Screen-edge correction applies only to supported RectTransform holders; larger sizes can crowd panels or clip artwork. Visual placement, animation, and input still need in-game testing.

## Mod menu (0.5.2)

Press **F10** to open or close the menu from gameplay or other game screens. You can also open Settings or pause the game and click **ULTRAWIDE MOD** at the bottom. Escape or Close dismisses the panel. Separate HUD and board sliders cover 100-135%; Reset restores 123%. Changes save immediately.

The overlay survives scene loads and closes on a scene change. Opening saves the current Unity time scale and cursor state, pauses the game clock, and suspends enabled StarVaders input-handler and key-controller components. Closing restores their prior states, including an already-paused clock. This does not stop coroutines driven by real time; pause and input behavior still require runtime checks.

The panel reuses loaded native slider assets and the game's font. Before native slider assets have loaded, it provides simple sliders using the same font; visiting Settings upgrades the style when the panel is closed. The game UI font must finish loading before the panel can be created. F6/F7 and F8 remain available. Controller navigation, pointer interactions, scene transitions, and exact visual placement are not yet verified in game.

### Menu rendering and transition fixes (0.5.2)

Labels now clone native text objects, using an unmasked font material and explicit shader channels. The open panel uses its own EventSystem and default Input System UI actions while the previous enabled EventSystems are suspended and restored on close. The pause screen now also exposes the menu entry. A Harmony prefix on CanvasScaler.HandleScaleWithScreenSize applies the ultrawide match before each canvas calculates its scale, including newly enabled canvases, instead of waiting for periodic discovery.

These are targeted fixes for the reported blank menu and transition zoom. Compilation is verified, but their visual and interaction results still need confirmation in game.

## Rebinding hotkeys (0.5.3)

All keyboard bindings are configurable. Defaults are:

| Action | Default |
| --- | --- |
| Decrease HUD and board size | F6 |
| Increase HUD and board size | F7 |
| Toggle fix | F8 |
| Write diagnostics | F9 |
| Open/close mod menu | F10 |

Close the game and edit `BepInEx/config/local.dicevaders.ultrawide.cfg`. The `[Hotkeys]` section is generated when 0.5.3 first starts:

```ini
[Hotkeys]
DecreaseSize = F6
IncreaseSize = F7
ToggleFix = F8
Diagnostics = F9
Menu = F10
```

Use Input System key names such as `F1`, `F12`, `PageUp`, `PageDown`, `Home`, or `End`. Set an action to `None` to disable its shortcut. Choose different keys for different actions. Bindings are single keys, without modifier combinations; rebinding is through the config file rather than a menu key-capture control. Restart to apply changes. The Settings and Pause menu entries remain available if the Menu shortcut is disabled.

Pause entry placement (0.5.4): the pause-screen button now sits in its own row above Game Menu, positioned relative to Resume. Settings keeps its existing entry location. Runtime visual confirmation is pending.
