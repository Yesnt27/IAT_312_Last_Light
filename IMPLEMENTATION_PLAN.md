# Implementation plan

This plan changes over time. Phases 1 onward get filled in once the assets and design context are provided.

## Phase 0: Tooling (done)
- `.mcp.json` connects Claude Code to MCP for Unity over HTTP at `127.0.0.1:8080/mcp`.
- `CLAUDE.md` records the project facts and the MCP workflow rules.

## Phase 1: Context intake (next)
These inputs are still needed:
- **Assets** in `Assets/Visual Assets/`: sprites, sprite sheets, tilesets, UI, fonts, and audio. Include the pixels-per-unit and the art style (pixel art or painted).
- **Story and scene flow:** the order of Title → Introduction → Outside → Basement → Floor 1–4 → Top Lighthouse, and what unlocks each step.
- **Puzzles and interactables for each scene:** what the Safe, ElectricalBox, Journal, FoundPage, and MessageInaBottle each do.
- **Where 3D appears.** Options: a separate 3D section, 2.5D (2D characters in 3D spaces), or 3D-only cutscenes.
- **Controls:** keyboard only or gamepad too, and the interact, journal, and pause keys.

## Phase 2: Foundations (shared by every scene)
1. Decide the render pipeline. URP is not installed. 2D lights and shadows suit a lighthouse game and need URP's 2D Renderer. Switching later forces material rework, so decide before building scenes.
2. `SceneController`: scene loading with a fade, named spawn points, and all scenes added to Build Settings.
3. Persistent `GameState`: story flags, inventory, and collected pages, kept across scenes with `DontDestroyOnLoad`.
4. Interaction system: an `Interact` action in `PlayerControls`, an `IInteractable` interface, a proximity prompt, and a player lock while dialogue or UI is open.
5. Prefabs: Player (with an Animator), Main Camera (consider Cinemachine for bounds and smoothing), Dialogue UI, and an interaction prompt.

## Phase 3: Asset pipeline
Set import settings (pixels-per-unit, point filtering for pixel art), slice sprites, build animator controllers, make tile palettes and tilemaps, and define sorting layers.

## Phase 4: Gameplay systems (the existing stubs)
- `Journal` and `FoundPage`: pages you can collect, plus a journal UI to read them.
- `MessageInaBottle`: a readable note shown in a pop-up.
- `Safe`: a code-entry puzzle UI that unlocks an item or flag.
- `ElectricalBox`: a power puzzle that toggles lights and doors.
- `Basement`: logic specific to the Basement scene.

## Phase 5: Scenes, built in story order
For each scene: lay it out from the assets, add colliders and ground, place interactables and dialogue, wire the transitions, then playtest. Order: Title Screen → Introduction → Outside → Basement → Floor 1 → 2 → 3 → 4 → Top Lighthouse.

## Phase 6: The 3D portion
To be decided in Phase 1.

## Phase 7: Polish and build
Audio, lighting, menus (pause, settings), save/load if needed, a windowed build, and a final playthrough.
