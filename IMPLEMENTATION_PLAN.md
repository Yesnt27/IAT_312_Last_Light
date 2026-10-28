# Implementation plan

Sources: the Figma frames "Full Gameplay" (585:300) and "Puzzles" (585:456) in *IAT312-Funsies*.

## Ground rules
- **Placeholders hold no story logic.** Placeholder art is a colored sprite with a text label. All behavior lives in reusable components on the object, so swapping in final art never breaks gameplay. Puzzle logic, such as the wires, is allowed because it's easy to change.
- **Environments are modular.** Each room is built from prefab blocks, and every object keeps its art and its collision in separate children (`Art` and `Collision`). Art can be replaced or resized without touching colliders, spawn points or triggers.
- **Content is data, not code.** Days, ship counts, journal entries, dialogue, passwords and wire colors are stored in ScriptableObjects or Inspector fields, so you can change them without editing scripts.
- **Cutscenes are text cards.** Wherever a cutscene plays, a full-screen card shows its name and description from Figma with a Continue button. Real cutscenes replace these later.
- **Each phase ends with a check and a commit.** I test in Test Scene first, read the Unity Console, and then ask before committing.

## The game loop (from Figma)
```
Title → Day 1 → Night 1 → Day 2 → Night 2 → Day 3 → Night 3 → Day 4 → Night 4 → Ending (Victory | Death)
```
**Each day:** wake up with a fade, then the day puzzle (it's mandatory and gives a journal page), then free exploring. A prompt sends you to the top of the lighthouse, where the night shift starts (guiding ships), followed by a night report and bedtime.

| Day | Morning | Day puzzle | Journal page | Night ships | Bedtime |
|---|---|---|---|---|---|
| 1 | Cutscene #1 (boat with the manager) → fade to the dock, dialogue | Explore, enter the lighthouse, find the living quarters and pick up the **journal** (gameplay unlocks) | Entry 1 | 3 | — |
| 2 | Wake; radio call about the food | **Puzzle 1, Basement Door:** search the drawers and cupboards for the key, open the basement, interact with the food | Entry 2 | 5 | Cutscene #2 (a shadow passes) |
| 3 | Wake; crash, lights go out; static on the radio | **Puzzle 2, Electrical Box:** open the box and match the colored wires | Entry 3 (taped inside the box door) | 7 | Cutscene #3 (noises) |
| 4 | Wake; a horn sounds outside | **Puzzle 3, Message in a Bottle:** find the bottle, find the right book, decipher the word, open the **safe** | Entry 4 | 9 | Cutscene #4 (radio: "pick you up in the morning") |
| End | Image cutscene: a ship arrives, horn | Missed too many ghost ships → **DEATH** (ghosts pull you onto the ship). Otherwise → **VICTORY** (sail away) | | | |

## Kept across scenes (DontDestroyOnLoad)
One `GameState` object survives scene loads. It holds:
- **The items you listed:** the basement key, the journal book and which pages are collected, and the message in a bottle (found or opened, and whether it's deciphered).
- **What the game flow needs:** the current day and phase, the active task, power on or off, whether the cipher book is found, whether the safe is opened, and the night results (ghost ships missed, false alarms, ships guided).

Nothing is saved to disk. Restarting the game resets everything.

## Scene layout (my proposal; easy to change)
| Scene | Contents |
|---|---|
| Title Screen | Start and Quit buttons |
| Introduction | Cutscene #1 card |
| Outside | Dock where you're dropped off, island walk, lighthouse door, water (the bottle on Day 4), horn trigger |
| Floor 1 | Entrance, **basement door** (locked), stairs |
| Floor 2 | **Living quarters**: bed (ends the day), **journal** pickup, radio |
| Floor 3 | **Book room**: bookshelf with the cipher book, and the **safe** (its prompt is always on) |
| Floor 4 | Storage: **drawers and cupboards** (one hides the key) |
| Basement | **Food** (page 2), **electrical box** (page 3, wires puzzle) |
| Top Lighthouse | Operating room: lamp switch, **alert button**, view of the sea for the night shift |
| *Ending (new scene)* | Victory or Death cutscene card, then back to the title |

---

## Phase A: URP 2D Renderer and lights ✅ done 2026-10-27
1. Create `Assets/Settings/Rendering/URP-2D.asset` together with its `Renderer2D` data.
2. Assign it under Graphics settings and on every Quality level.
3. In all 10 scenes:
   - remove the 3D `Directional Light`
   - add a `Global Light 2D` (intensity 1)
   - set the camera to orthographic
   - add a global post-processing `Volume`, which the sanity effect needs later
4. Commit, including the URP package you already installed.
- **Check:** Test Scene renders normally in Play mode, and the Console has no errors.

## Phase B: Core systems (built and tested in Test Scene) ✅ done 2026-10-27
All 10 scenes were also added to Build Settings (needed for scene loading).

All of these live on one **`Systems` prefab** that is never destroyed. A small `Bootstrap` script creates it if it's missing, so you can press Play from any scene.

| System | Script (folder in `Scripts & Controllers/`) | What it does |
|---|---|---|
| Game state | `Core/GameState.cs` | The persistent data listed above, plus change events that UI and lights listen to |
| Scene flow | `SceneController.cs` (fills the existing stub) | Loads scenes behind a fade, then places the player at a named `SpawnPoint` |
| Transitions | `Core/SceneTransition.cs`, `Core/SpawnPoint.cs` | Doors and stairs: interact to move to another scene and spawn point |
| Fader | `UI/ScreenFader.cs` | Fade to and from black (waking up, scene changes) |
| Dialogue | `Dialogue Stuff/DialogueManager.cs` (upgrade) | Enter advances lines (this isn't wired up today); lines without a speaker for sound cues like *\*Static\**; an on-finished callback for chaining events; freezes the player while text is open; a trigger option to fire only once |
| Tasks | `UI/TaskDisplay.cs` | The "Task Dialogue Box": shows the current objective on screen |
| Interaction | `Interaction/Interactable.cs`, `Interaction/InteractionPrompt.cs`, `Interaction/PlayerInteractor.cs` | Shows an "E" prompt near interactables. Each one has **visibility conditions** (which days and tasks it appears for), as in Figma's "prompts only show up during this puzzle". **Locked** interactables play a "you need something" line |
| Cutscene card | `UI/CutscenePlaceholder.cs` | Full-screen "CUTSCENE #n: …" text with a Continue button, and a callback when it closes |
| Player | `PlayerController.cs` (fixes) | Can be frozen during dialogue, UI and cutscenes. Becomes a prefab with Rigidbody2D, collider and ground check. The current Test Scene "Square" can't run: it has no Rigidbody2D and also has the DialogueManager on it |
| Camera | `CameraManager.cs` (small upgrade) | Follows the player and is clamped to each room's `RoomBounds`, so a room can change size freely |

- **Check:** in Test Scene, walk, interact, read dialogue, see a task update, play a placeholder cutscene, and travel between two scenes with the key flag still set.

## Phase C: Day and night flow ✅ done 2026-10-27
Also added early: a minimal `FoundPage` (collect page N; the journal UI is still Phase E), `Bed`, `NightShiftStart`, and the `DayDebugStart` dev tool. The night is a placeholder card plus a report card until Phase G. The Victory/Death endings are cards for now, followed by the Title Screen (the Ending scene is still Phase H).

- **`Core/DayDefinition.cs` (ScriptableObject), one asset per day.** Fields:
  - the wake-up dialogue
  - the day puzzle's task text
  - which journal page that day awards
  - the night ship count (3, 5, 7, 9)
  - the bedtime cutscene text
  - whether the day starts with a power outage

  The day count is just the length of this list, so going from 4 days to 3 is a data change, not a code change.
- **`Core/DayManager.cs`** steps through the phases of each day:
  - Wake: fade in and play the wake dialogue.
  - Day puzzle: until that day's page is collected.
  - Explore: free roaming, with a prompt to go to the top of the lighthouse.
  - Night: shift in Top Lighthouse, then the report.
  - Bedtime: use the bed, then the bedtime cutscene card, then the next day.

  After the last day it goes to Ending.
- **Day 1 is special:** before you pick up the journal, everything except doors is non-interactive, as in Figma.
- **Check:** with the puzzles stubbed out, run through all 4 days on placeholder content.

## Phase D: Placeholder environments (started)
So far: the player auto-spawns (no Player in scenes), and the Environment prefabs exist (Door, Stairs, Spawn Point, Ground, Wall). Every room is now connected and walk-tested: Outside ⇄ Floor 1 ⇄ 2 ⇄ 3 ⇄ 4 ⇄ Top Lighthouse, plus Floor 1 ⇄ Basement, which is locked until `BasementKey`. Pressing Play in Outside starts a new game; the title screen comes later. Items (bed, journal, pages, key, lamp controls) are left for the user to place. Outside and Top Lighthouse still have hand-placed Players from the user.

- **Modular prefabs** in `Assets/Prefabs/Environment/`: `Floor`, `Wall`, `Platform`, `Stairs` (a transition), `Door` (a transition, optionally locked), `RoomBounds`, `WaterZone`. Each one has `Art` and `Collision` children.
- **Interactables:** a placeholder prefab for each interactable (sprite plus a world-space label such as "DRAWER" or "SAFE"). Their logic components are added separately.
- **Placeholder sprites:** colored Square and Circle sprites in `Assets/Visual Assets/Placeholders/`.
- **Building the scenes:** every scene in the table above gets floor, walls, spawn points, transitions and room bounds, with the right objects placed.
- **Check:** walk from the dock to the top of the lighthouse and down to the basement without falling through anything.

## Phase E: Journal and pages
- **`Journal/JournalPage.cs` (ScriptableObject):** entry number, title, body text. Placeholder text comes from the Figma notes: Entry 1 is about the island and the weird ship; 2 is sounds at night; 3 is a breakdown; 4 is drawings of the same ship.
- **`Journal.cs` (fills the stub):** the journal pickup on Day 1, plus the journal UI.
  - Open it with **J** (I'll add a `Journal` input action).
  - Uncollected pages show as "MISSING PAGE".
- **`FoundPage.cs` (fills the stub):** a reusable "award page N" component, used by the food, the electrical box and the safe.
- **Check:** pick up the journal, collect a page in another scene, and the page appears in the journal UI.

## Phase F: The three day puzzles (in progress)
**Update 2026-10-27:** the story was redefined into Days 1–3 (see CLAUDE.md, "The story so far"). Day 1 is the basement key + food + journal, Day 2 is the power and wire puzzle (built: `Wire Puzzle` prefab + `ElectricalBox`), and Day 3 is the bottle → book. The safe and the Day 3 ending are still to do. Reaching the top of the lighthouse ends the day while the night shift isn't built. The original plan below is kept for reference.

### Puzzle 1: Basement Door (Day 2)
- **Opening:** the wake-up radio call plays the Figma dialogue, then the task "Check around the house to find the key for the basement".
- **Searching:** `Puzzles/SearchableContainer.cs` is used on every drawer and cupboard, and the Inspector marks which one has the key. The others say *Drawer is empty* or *Cupboard is locked*. These prompts only appear during this task.
- **Basement door:** a locked `Door` that needs `hasBasementKey`.
- **Ending the puzzle:** the food in the basement (`Basement.cs`, filling the stub) awards page 2.

### Puzzle 2: Electrical Box (Day 3)
- **Opening:** the outage sets `powerOn = false`. `Lighting/PowerLighting.cs` dims every Global Light 2D in the lighthouse scenes. The crash, static and task dialogue then play.
- **Opening the box:** `ElectricalBox.cs` (fills the stub). Its prompt only appears during this task. Opening it awards page 3 (taped to the door) and opens the wires panel.
- **Wires puzzle:** `Puzzles/WirePuzzle.cs` is a UI panel. You drag from each colored wire end on the left to the matching color on the right. The number of wires, their colors and the shuffle are set in the Inspector.
- **Ending the puzzle:** when it's solved, the power comes back, then the lights-on sound cue line and the radio "Glad you were able to fix it" dialogue play.

### Puzzle 3: Message in a Bottle (Day 4)
- **Opening:** the horn cue plays, then the task "Check the horn sound coming from outside". A trigger at the shore plays the line about there being no ships, then the task "Observe what is floating in the water".
- **The bottle:** `MessageInaBottle.cs` (fills the stub). The bottle only exists on Day 4. Opening it shows a UI with the message in placeholder symbols and sets `bottleFound`.
- **The bookshelf:** `Puzzles/Bookshelf.cs`, active only during this section. It holds several books; the right one is set in the Inspector, and the wrong ones play a short line. The right book opens a cipher card mapping symbols to letters. The message then counts as deciphered, followed by the dialogue and task "Figure out what this message can be used for".
- **The safe:** `Safe.cs` (fills the stub). Its prompt is always on. You type the password word, which is set in the Inspector. The right word opens the safe and awards page 4. The Figma alternative, dragging symbols into slots, can replace the typing input later without changing the safe's unlock logic.

- **Check:** play each puzzle start to finish from its day's wake-up.

## Phase G: Night shift (the main mechanic, in Top Lighthouse)
- **`Night/ShipTrait.cs` (ScriptableObject).** One asset per possible difference from Figma:
  - seaweed, glowing, cannon number, sail number, symbol, colour
  - ship code, ship type, number of valves, sail shape, ghost figure (shown briefly)

  Each trait gets a placeholder visual on the ship prefab.
- **`Night/ShipSpawner.cs`:** spawns the night's ships one at a time, with a configurable share of ghost ships. A ghost ship carries "ghost traits" drawn from the traits the collected journal pages have described.
- **`Night/Ship.cs`:** sails across the view. When it leaves the screen, the result is decided:
  - **normal ship + light on** → guided
  - **ghost ship + light off + alert pressed** → caught
  - **ghost ship let through** → missed (sanity −1)
  - **alert on a normal ship** → false alarm
- **`Night/LighthouseControls.cs`:** lamp on/off switch and the alert button, as UI buttons or world interactables.
- **`Night/NightReport.cs`:** a summary card after each night, with a placeholder audio cue if you made a mistake.
- **`Night/SanityEffect.cs`:** with each missed ghost ship, it strengthens the vignette and lowers the saturation on the global Volume, in every scene. This is visual only, as in Figma.
- **Check:** play nights with 3 and 9 ships and confirm the report counts and the sanity visuals.

## Phase H: Endings, title, build
- **Ending scene:** "Image cutscene: ship arrives". Then a **Death** or **Victory** card, decided by `missedGhostShips` against a threshold set in the Inspector. Then back to the title.
- **Title Screen:** Start (resets GameState and goes to Introduction) and Quit.
- **Build settings:** add every scene in order.
- **Check:** a full playthrough to Victory and another to Death.

---

## Decisions I need from you (I'll use these defaults if you don't say otherwise)
1. **3 or 4 days?** Figma says "start with 3 days", but the narrative has 4 days and 4 ship counts. *Default: 4, and it's a data change either way.*
2. **How you spot a ghost ship.** *Default:*
   - Each journal page reveals one or two ghost traits.
   - A ghost ship always shows at least one trait you've already learned.
   - False alarms appear in the report but don't count toward death.
   - **3 or more** missed ghost ships leads to the Death ending.
3. **Which room goes on which floor.** *Default: the scene table above.*
4. **The safe.** *Default: type the deciphered word. The symbol-drag version is the "alternative" in Figma.*
5. **Radio calls.** *Default: they play automatically at wake-up as dialogue. The radio is a prop in the living quarters.*
6. **3D.** The Figma doesn't mention 3D. *Default: all 2D for now.*
7. **Book vs. journal.** I read "Book & Book Pages" as the journal and its pages. The cipher book from Puzzle 3 is also kept as a flag.
