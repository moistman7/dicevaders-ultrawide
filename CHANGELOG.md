# Changelog

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
