# Changelog

## 0.6.5 - 2026-10-19 (prerelease)

- Restore the block font's previously working nominal text sizes.
- Enlarge Pause text to 170% through label transforms instead of requesting normalized glyph sizes.
- Skip glyph-height requests for the block font; use transform scaling for block headings as well.
- Keep the working Options body font resolution, controls, hotkeys, and saved sizes.
- Build passed; pause visibility and visual size still need an in-game check.


## 0.6.4 - 2026-10-19 (prerelease)

- Normalize native font sizes using measured glyph height, with an additional 15% readability increase for Pause.
- Enlarge the pause entry and all pause-panel text without changing layout or controls.
- Cache native font sources before Settings assets strip their source-file links, and resolve named font families.
- Search Settings body/action labels when ApplyButton does not expose usable text.
- Keep hotkeys, saved sizes, and menu exit handling.
- Build passed; startup used the copied StarvadersGun font, and font-lifetime plus button click/raycast checks passed.


## 0.6.3 - 2026-10-19 (prerelease)

- Match the pause entry and panel to Resume's native source font.
- Match the Settings entry and body/buttons to Apply & Exit's source font, with the Settings heading font for the panel title.
- Own protected copies of native Font objects instead of borrowing scene-owned source objects; retain Arial as fallback when a source font is unavailable.
- Add an opt-in font-copy lifecycle check across unused-asset unloading; the loaded native font passed it, and button raycast/callback checks passed.
- Preserve layout, controls, hotkeys, and saved sizes.


## 0.6.2 - 2026-10-19 (prerelease)

- Hide the mod entry immediately when Options SaveAndExit starts and when Settings disables.
- Suppress entry rediscovery during the closing Settings screen's transition; clear that state when Settings reopens.
- Keep button input, shared styling, hotkeys, and saved sizes unchanged.
- Build passed; exit-transition timing needs an in-game check.


## 0.6.1 - 2026-10-19 (prerelease)

- Give buttons standard transparent Unity Image hit targets; custom geometry is visual-only.
- Add CanvasRenderer explicitly for custom artwork and bypass native color-tween initialization on the injected graphic.
- Apply the fix to the shared Settings, Pause, and hotkey panel without changing its appearance.
- Add an opt-in runtime raycast and native pointer-click check for Toggle, Reset, and Close, restoring saved settings afterward.
- Build verified.


## 0.6.0 - 2026-10-19 (prerelease)

- Hook Settings OnEnable and Pause OnPause directly to prepare entries without waiting for the discovery timer.
- Prefer active/paused controllers over inactive duplicates.
- Replace scene-dependent sprite selection with one shared geometric skin based on the preferred Settings layout.
- Render angled backdrop, slim teal buttons, blue filled sliders, and slash handles in every context.
- Preserve owned font, working control logic, configurable hotkeys, saved sizes, and 123% defaults.
- Build and startup registration/text/state checks passed. Visual styling and lifecycle timing need user confirmation; hidden-window captures are black.


## 0.5.9 - 2026-10-19 (prerelease)

- Restore sliced rendering for native button/panel images after the 0.5.8 simple-rendering regression.
- Select the effective displayed sprite (including overrides), skipping empty images.
- Protect referenced UI sprites/textures from unused-asset unloading.
- Preserve generated fallback padding with FullRect sprite geometry.
- Keep the working owned font, hotkeys, and saved sizes.
- Build verified; visual confirmation pending.


## 0.5.8 - 2026-10-19 (prerelease)

- Replace the plain Pause fallback with Settings-style angled panel shapes, blue filled sliders, slash handles, and slim teal buttons.
- Remove the green Resume artwork from the pause entry.
- Generate original fallback textures locally so the style is available before Settings loads.
- Preserve native Settings assets, reliable owned text, saved sizes, and hotkeys.
- Build verified; new fallback styling still needs an in-game visual check.


## 0.5.7 - 2026-10-19 (prerelease)

- Stop borrowing scene font objects after the log confirmed a destroyed-font exception.
- Give the overlay an owned Arial font protected from unused-asset unloading, with recovery if it is lost.
- Remove the pause entry's native-font override.
- Prebuild the overlay and cache inactive menu controllers; check visibility every frame to remove the delayed pause entry.
- Preserve direct opening, native artwork where available, hotkeys, saved sizes, and 123% defaults.
- Build verified; visual and pause-screen interaction verification pending.


## 0.5.6 - 2026-10-19 (prerelease)

- Remove the Options-opening workaround: Pause and the menu hotkey open the standalone panel directly.
- Replace cloned TMP labels with fresh Unity UI Text controls, using the game's source font when available and Arial as a fallback.
- Reset cloned control depth and log menu text geometry on opening.
- Add an opt-in startup smoke check for text generation and state restoration.
- Preserve saved sizes, configurable hotkeys, and 123% defaults.
- Startup runtime check generated 44 HUD-label vertices using StarvadersGun-Regular; closing restored timeScale to 1.


## 0.5.5 - 2026-10-19 (prerelease)

- Style the pause entry using native Resume button artwork and font.
- Route the pause entry through the game's Options flow, then open the same mod panel used in Settings.
- Rebuild the panel from Settings assets when first arriving from Pause, avoiding a different fallback appearance.
- Preserve configurable hotkeys, saved sizes, and the 123% default.
- Build verified; pause-to-Settings flow still needs runtime confirmation.


## 0.5.4 - 2026-10-19 (prerelease)

- Give the pause-menu entry a separate position above Game Menu, relative to the Resume button, instead of overlapping Resume and Options.
- Preserve the working Settings entry and panel layout, saved sizes, and configurable hotkeys.
- Build verified; pause-entry placement still needs an in-game visual check.


## 0.5.3 - 2026-10-19 (prerelease)

- Make decrease, increase, toggle, diagnostics, and menu shortcuts configurable in a Hotkeys config section.
- Keep F6-F10 defaults; support None to disable an action.
- Show the selected menu key in its footer.
- Clarify that NorthernDale and GitHub moistman7 are the same mod author; update assembly author metadata.
- Build verified; runtime verification pending.


## 0.5.2 - 2026-10-19 (prerelease)

- Add the mod menu entry to the pause screen.
- Clone native labels with an unmasked font material and explicit canvas shader channels to address blank text.
- Give the open menu independent UI input, restoring previous EventSystems on close.
- Hide managed-only helper signatures from IL2CPP injection.
- Apply ultrawide scaling before each CanvasScaler height/width calculation to address scene-transition zoom.
- Keep saved sizes and 123% defaults.
- Build verified; runtime confirmation pending.


## 0.5.1 - 2026-10-19 (prerelease)

- Make F10 available outside Settings, including before Settings has been visited.
- Keep the Ultrawide Mod entry in Settings.
- Preserve the overlay across scenes and close it on scene changes.
- Save and restore Unity time scale, cursor state, and suspended game input handlers.
- Provide fallback sliders until native settings assets become available.
- Keep 123% defaults and existing saved preferences.
- Build verified; runtime pause/input and UI verification pending.


## 0.5.0 - 2026-10-19 (prerelease)

- Save the chosen 123% HUD and board sizing as defaults for new configurations.
- Add an Ultrawide Mod panel in Settings using the game's slider artwork and font.
- Add independent sizing sliders, an enable button, and Reset to 123%. Changes save immediately.
- F10 opens the panel while Settings is visible; F6/F7 and F8 remain available.
- Build verified; menu layout and interaction testing pending.


## 0.4.1 - 2026-10-19 (prerelease)

- Restore optional HUD and board sizing with F6/F7 controls.
- Default both multipliers to 100% to preserve the original appearance.
- Keep independently configurable sizes and F8 restoration.
- Build verified; runtime validation pending.


## 0.4.0 - 2026-10-19 (prerelease)

- Remove experimental panel and board enlargement to preserve the original game's UI proportions.
- Remove per-frame transform adjustments, bounds correction, F6/F7 sizing controls, and the game-specific assembly dependency.
- Keep height-based ultrawide canvas scaling, F8 toggle, and resolution handling.
- Build verified; runtime visual comparison pending.


## 0.3.0 - 2026-10-19 (experimental prerelease)

- Add encounter HUD and board sizing, defaulting to 112% and 118%.
- Add F6/F7 size adjustment with saved settings and F8 restoration.
- Add bounds correction for supported RectTransform holders.
- Keep enlargement separate from canvas-match settings to reduce global zoom clipping.
- Build verified; runtime layout, animation, and input validation pending.


## 0.2.0 - 2026-10-19 (prerelease)

- Reduce canvas discovery from 20 to 4 scans per second.
- Track original scaling with canvas references; prune destroyed canvases and handle recycled instance IDs.
- Restore tracked scaling when the behaviour is destroyed.
- Bound canvas-match and startup timing settings; throttle repeated exception logs.
- Resolve automatic width and height independently.
- Apply fullscreen-mode changes even when dimensions already match; validate numeric enum values.
- Stop resolution enforcement while the fix is disabled.
- Remove automatic installation from the build and exclude runtime dependency copies.
- Add source documentation and a plugin-only release package.

Build verified; this revision has not yet been tested in game. Tutorial alignment and bottom-panel clipping are not claimed fixed.
