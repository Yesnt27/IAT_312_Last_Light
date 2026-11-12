# Sound effects

All sound effects go through one player: **Sound Effects** on the Systems prefab (`Audio/SoundEffects.cs`). It plays a sound once at an even volume, not tied to a position, and the sound keeps going even if the scene changes. Its **Master Volume** turns every sound effect up or down at once.

Your sound files are in `Assets/Sounds/` (`cupboardOpenClose`, `safeOpen`, `pageFlip`, `horn`, `ghost`, `woosh`, `tick`, `walk`...). Drop new `.mp3` / `.wav` files in there too.

**Already hooked up:**
- **Floor 4's cupboards and drawers** play `cupboardOpenClose` when opened and when closed.
- **The safe** plays `safeOpen` when the right code is typed in.

There are three ways to add a sound, from easiest to most flexible.

## 1. Anything you can interact with: Interact Sound (no code)

Every object you press E on or click (cupboards, doors, pages, the bed, the bookshelf, the electrical box, the bottle...) has a **Sound (optional)** section in its Inspector:

| Field | Plays when |
|---|---|
| **Interact Sound** | it's used (each time) |
| **Locked Sound** | it's tried while locked (e.g. the basement door before you have the key) |
| **Sound Volume** | volume for both, 0–1 |

**Example, the cupboards:**
1. Open Floor 4 and select `Find Key/Cabinet Controller 1`.
2. On the **Cabinets** component, find **Sound (optional)**.
3. Drag `Assets/Sounds/cupboardOpenClose` into **Interact Sound**.

All four cupboard and drawer controllers are set up this way.

**More ideas:**
- **Basement door:** put a locked rattle in **Locked Sound**.
- **Journal Page prefab** (`Prefabs/Interactables/Journal Page`): put `pageFlip` in **Interact Sound**, and every page you pick up rustles.

## 2. Any other event: the Play Sound component (no code)

For things that aren't "interact with this", add a **Play Sound** component (`Audio/PlaySound.cs`) and call it from an event. Its settings:
- **Clip** and **Volume**.
- **Play On Enable** (optional): plays whenever the object is switched on.

To hook it to an event:
1. Select the object that has the event, or any object in the same scene, and **Add Component → Play Sound**. Set its **Clip**.
2. In the event list (the box with a **+**), press **+**.
3. Drag the object with the Play Sound into the slot.
4. Pick **PlaySound → Play()** from the dropdown.

Events you can use this way:
- **On Interact** on any Interactable. Use it when you want more than one sound, or a sound plus something else.
- **CameraTrigger → On Zoom Out / On Zoom In** (Outside's zoom).
- **PlayOncePerGame → On First Time**.
- A UI **Button → On Click** (Title Screen buttons, the bookshelf's X...).
- **Play On Enable:** for something that appears, like a panel opening.

## 3. In a script: one line

```csharp
[Header("Sound")]
[SerializeField] private AudioClip openedSound;   // drag the clip in on the Inspector

// where the event happens:
SoundEffects.Play(openedSound);          // or SoundEffects.Play(openedSound, 0.5f) for half volume
```

It's safe with an empty clip (nothing plays). This is how the safe does it: `Safe.cs` → `OnCorrectCode()` → `SoundEffects.Play(openedSound)`, with **Opened Sound** = `safeOpen` on Floor 4's Safe.

**Good places to add a line like this:**

| Sound for | Script → method |
|---|---|
| Journal page turn | `UI/JournalView.cs` → `ShowPage` |
| Wire connected / puzzle solved | `Puzzles/WireDragEnd.cs` → `TryConnect`, `ElectricalBox.cs` (solved) |
| Keypad key press / wrong code | `Puzzles/SafeKeypad.cs` |
| Opening a book on the shelf | `Puzzles/BookshelfPuzzle.cs` → `OpenBook` |
| Ship reported / ghost sinking | `Night/NightShip.cs` |
| Night timer ticking | `Night/NightShift.cs` (`tick`) |

## Ambience (looping background sound per scene)

**Ambience** on the Systems prefab (`Audio/AmbiencePlayer.cs`) plays a looping sound for each scene:

| Entry | Scenes | Clip | Volume |
|---|---|---|---|
| Indoors | Floor 1, Floor 2, Floor 4, Basement, Top Lighthouse | `Sounds/ambience` (3:53) | 0.6 |
| Outside | Outside | `Sounds/u_jt8c4ibcgy-beach-sound-with-seagulls…` (19 s, loops) | 0.6 |

**How it behaves:**
- **Same clip:** walking between floors that share a clip keeps it playing without restarting.
- **Different clip:** going out the front door crossfades from the indoor ambience to the beach over **Fade Seconds** (1.5).
- **No entry:** a scene in no list fades to silence. That covers the Night Scene, the Title Screen, How To Play and the cutscene scenes.

**To change it:** open `Assets/Prefabs/Resources/Systems` → **Ambience** → **Ambiences**.
- **Name** is just a label.
- **Scenes** take scene names exactly as spelled in `Assets/Scenes`.
- Set the **Clip** and its **Volume**.
- Add an entry with **+** (e.g. `ocean` for the Night Scene, or `badWeather` for a stormy day).
- **Master Volume** turns all ambience up or down.

Volumes can be changed while playing (see the next section for *where*).

## "I move the volume slider and nothing changes"

While the game is running, the Systems prefab you see in the Project window is **not** the copy that's playing. The game makes a copy called **Systems(Clone)** in the Hierarchy, under **DontDestroyOnLoad**.
- **Prefab changes during Play** go to the file, not the running copy, so you hear no difference until the next Play.
- **To try volumes live:** expand DontDestroyOnLoad → Systems(Clone) → **Sound Effects** (or **Ambience**) and drag the slider there. Unity throws those changes away when Play stops, so note the value and set it on the prefab afterwards.
- **One-shot sounds** (cupboard, safe) only use the new volume the **next** time they play. A sound that's already playing keeps its volume.
- **The sliders only go down:** 1 = the clip as recorded. If a clip is too quiet even at 1, make the file louder (any audio editor) or lower the other sounds.

## Story sounds that loop (the Day 4 horn)

**Day 4 Horn** on the Systems prefab (`Audio/StoryLoopSound.cs`) loops `Sounds/horn` from the start of Day 4, while the player is inside the lighthouse (Floor 1, 2, 4, Basement, Top Lighthouse). As soon as they step **Outside** it fades out (1 s) and sets the flag `HornStopped`, so it doesn't come back that game, even if they go back in. A new game clears the flag.

Its fields:

| Field | Meaning |
|---|---|
| **Clip / Volume / Fade Seconds** | the sound, how loud (0.8), fade in/out time |
| **Active On Days / Active During Tasks / Required Flags / Hidden By Flags** | when it may play, same as on an Interactable (now: Day 4) |
| **Plays In Scenes** | where it may play; empty = everywhere |
| **Stop For Good Flag** | set when the player leaves those scenes while it plays; then it stops for the rest of the game. Empty = it comes back when they return |

For another looping story sound (a ghostly whisper on Night 3, rain on one day...), select `Day 4 Horn` in the Systems prefab, press **Ctrl+D**, rename the copy and change its fields.

## Added for the finishing touches

- **Binoculars:** `tick` plays when they're raised to the eyes and when they're lowered. Night Scene → **Binoculars → Zoom Sound / Zoom Sound Volume** (2, because the file is quiet).
- **Storm ambience:** Systems → Ambience has a third entry, **Night storm**: `badWeather` loops in the Night Scene on every night, volume 0.45.
- **Footsteps:** `Audio/WalkSound.cs` on the **Player** prefab and on the Night Scene's **First Person Player** loops `walk` while the player walks on the ground. **Volume** (0.5) and **Fade Seconds** are on the component.
- **Louder effects:** Sound Effects → **Master Volume** now goes up to 3 and is 1.5 (it was 0.81, and its Audio Source's own Volume was 0.6, so effects played at about half strength; the Audio Source is back at 1). Each interactable's **Sound Volume** also goes up to 3 for a single quiet clip.

## Music and looping sounds

Background music and ambience (ocean, wind) are different: they loop and fade. Use an **Audio Source** on an object in the scene, with **Loop** and **Play On Awake** ticked. Outside's `zoom_music` adds **Music Fader** (`Audio/MusicFader.cs`) for fading in and out.

In the 3D Night Scene, an Audio Source with **Spatial Blend** set to 3D sounds like it comes from that spot (e.g. a horn out at sea).
