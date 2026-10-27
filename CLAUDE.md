# Lighthouse adventure game (Unity)

Instructions for Claude Code working in this project. Claude reads this file at the start of every session.

## Project facts
- Unity 6 (6000.6.2f1). 2D side-view so far (2D Sprite + Tilemap packages, Rigidbody2D). The 3D parts are not decided yet.
- Rendering: URP with the 2D Renderer (`Assets/Settings/Rendering/URP-2D.asset`, used by Graphics and every Quality level). Every scene has a `Global Light 2D`, an orthographic `Main Camera` with post-processing on, and a `Global Volume` using `GlobalVolumeProfile.asset` (Vignette + Color Adjustments, both neutral, for the sanity effect later). Sprites need `Sprite-Lit-Default` to react to 2D lights. Don't add 3D `Light` components.
- Input: the new Input System. `PlayerControls` is generated from `Assets/Scripts & Controllers/Input Action/PlayerControls.inputactions`. Its action map is `Controls` (`Move`, `Jump`). Never edit `PlayerControls.cs` by hand: change the `.inputactions` asset and let Unity regenerate it.
- Unity 6 physics API: use `rb.linearVelocity`, not `rb.velocity`.
- Scripts go in `Assets/Scripts & Controllers/`, grouped in subfolders by system (`Dialogue Stuff/` is the existing example).
- Art goes in `Assets/Visual Assets/`.
- Scenes are in `Assets/Scenes/`: Title Screen, Introduction, Outside, Basement, Floor 1–4, Top Lighthouse, and Test Scene (a sandbox).
- Existing systems: `PlayerController` (move and jump with a ground check), `CameraManager` (follows the player), `DialogueManager` singleton plus `DialogueTrigger` (typewriter text in TMP, starts on trigger enter with the `Player` tag).
- These are empty stubs waiting to be implemented: `SceneController`, `Basement`, `ElectricalBox`, `Safe`, `Journal`, `FoundPage`, `MessageInaBottle`.

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
