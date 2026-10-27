# Lighthouse adventure game (Unity)

Instructions for Claude Code working in this project. Claude reads this file at the start of every session.

## Project facts
- Unity 6 (6000.6.2f1). 2D side-view so far (2D Sprite + Tilemap packages, Rigidbody2D). The 3D parts are not decided yet.
- Rendering: URP with the 2D Renderer (`Assets/Settings/Rendering/URP-2D.asset`, used by Graphics and every Quality level). Every scene has a `Global Light 2D`, an orthographic `Main Camera` with post-processing on, and a `Global Volume` using `GlobalVolumeProfile.asset` (Vignette + Color Adjustments, both neutral, for the sanity effect later). Sprites need `Sprite-Lit-Default` to react to 2D lights. Don't add 3D `Light` components.
- Input: the new Input System, asset `Assets/Scripts & Controllers/Input Action/PlayerControls.inputactions`. Its action map is `Controls`: `Move` (A/D), `Jump` (Space/W), `Interact` (E), `Dialogue` (Enter, advances dialogue and closes cutscene cards). **Generate C# Class is off**, so there is no `PlayerControls` class. Scripts take `[SerializeField] private InputActionReference xAction;` fields, call `xAction.action.Enable()` in `OnEnable`, and only unsubscribe (never `Disable()`) in `OnDisable`, because actions are shared between scripts. Assign the references on the prefab (Player, Systems) by dragging the action sub-asset from the `.inputactions` file.
- Unity 6 physics API: use `rb.linearVelocity`, not `rb.velocity`.
- Scripts go in `Assets/Scripts & Controllers/`, grouped in subfolders by system (`Dialogue Stuff/` is the existing example).
- Art goes in `Assets/Visual Assets/`.
- Scenes are in `Assets/Scenes/`: Title Screen, Introduction, Outside, Basement, Floor 1–4, Top Lighthouse, and Test Scene (a sandbox).
- **Systems prefab** (`Assets/Prefabs/Resources/Systems.prefab`): `Bootstrap` spawns it before the first scene loads and it is never destroyed, so Play works from any scene. Never place it in a scene. It holds `GameState` (flags, pages, day, power, task, night results; flag names in `GameFlags`), `SceneController` (`LoadScene(scene, spawnId)` with a fade), the EventSystem, and the UI canvas with `DialogueManager`, `TaskDisplay`, `InteractionPrompt`, `CutscenePlaceholder` and `ScreenFader`.
- **Player prefab** (`Assets/Prefabs/Player.prefab`): tagged `Player`, sprite on an `Art` child (the root scale is flipped for facing, so never scale the root), `PlayerController` + `PlayerInteractor`. Ground colliders go on the `groundLayer` layer.
- **Per scene:** a `Player` instance, `CameraManager` on the Main Camera, one `RoomBounds` (camera limits) and `SpawnPoint`s with ids that `SceneTransition`s point at.
- **Interactables:** subclass `Interaction/Interactable` (needs a trigger Collider2D). Its Inspector conditions (days, active tasks, required/hiding flags) decide when the prompt shows, and `Unlock Flag` + `Locked Dialogue` make it locked. Ready-made: `InteractDialogue`, `FlagPickup`, `SceneTransition` (doors/stairs). Anything that should freeze the player uses `PlayerLock.Add/Remove`.
- `DialogueManager.StartDialogue(dialogue, onComplete)`: a line with an empty character name shows no speaker (use for sound cues like *\*Static\**). Starting a dialogue while another is open replaces it, so wait a frame and for `isDialogueActive` to clear before chaining.
- **Day loop** (`Day Flow/`): `DayManager` (on Systems) runs wake → day puzzle → explore → night → bedtime → next day → ending. Each day is a `DayDefinition` asset in `Assets/Data/Days/` (start scene/spawn, power outage, wake dialogue, puzzle task, journal page, ship count, bedtime card); the day count is the length of the list on DayManager. The day puzzle ends when that day's journal page is collected (`FoundPage`). Task ids DayManager sets: `go_to_lighthouse`, `night_shift`, `go_to_bed`. `NightShiftStart` (lamp controls) and `Bed` call it; `CutscenePlaceholder.Show(...)` shows "CUTSCENE:" cards, `ShowCard(...)` plain cards. The night is still a placeholder card until Phase G. `DayDebugStart` in a test scene runs the loop there without loading story scenes (Test Scene has one; disable it to test other things).
- Placeholder art uses `Assets/Visual Assets/Placeholders/Square.png`, never sprites from a package `Editor` folder (those are stripped from builds).
- These are empty stubs waiting to be implemented: `Basement`, `ElectricalBox`, `Safe`, `Journal`, `MessageInaBottle`.
- Testing over MCP: Play mode only advances while the Unity window has focus. If `Time.frameCount` stops moving, bring Unity to the front.

## Code style
Match `PlayerController.cs`: use `[Header]` groups, keep fields `[SerializeField] private`, use camelCase for fields and PascalCase for methods, and add few comments. Prefer small single-purpose MonoBehaviours over large managers.

## Unity MCP workflow (MCP for Unity, server "UnityMCP")
The Unity Editor must be open with the MCP server running (Window > MCP for Unity > Start Server).

1. **Check the connection first.** At the start of a task, call `manage_editor` or `manage_scene` (get active scene) to confirm Unity responds and the right scene is open.
2. **Scripts: write the file, then compile.** Create and edit `.cs` files directly on disk (or with `manage_script`). Then call `refresh_unity` and check `read_console` for compile errors. Do not attach a component until the code compiles cleanly.
3. **Scene objects: use the MCP tools.** Use `manage_gameobject`, `manage_components`, `manage_prefabs`, and `manage_ui` to create objects, add components, and assign serialized references. Never hand-edit `.unity`, `.prefab`, or `.asset` YAML.
4. **Reuse through prefabs.** Anything placed in more than one scene (the player, the dialogue UI, interactables) becomes a prefab under `Assets/Prefabs/`.
5. **Save and verify.** Save the scene with `manage_scene`. Re-read the hierarchy or components to confirm the change. Check `read_console` again after entering Play mode if behavior was changed.
6. **Experiment in Test Scene.** Try new mechanics in `Test Scene`, then move them into story scenes once they work.
7. **Commit at checkpoints.** Ask before committing. Scene changes are hard to diff, so commit before large scene edits.
