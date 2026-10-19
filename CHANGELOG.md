# Changelog

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
