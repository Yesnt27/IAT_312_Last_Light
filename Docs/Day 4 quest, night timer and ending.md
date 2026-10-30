# Day 4 quest, night timer and ending

This covers what was built for the end of the game, and where to change each part yourself:
- the night time limit
- the message in a bottle → book → safe quest (Day 4)
- the bigger last night
- the ending scene

It also covers re-applying the Day 1 settings that a merge had emptied.

## The whole Day 4, in order
1. Wake up in the Floor 4 bed to the horn. Go outside, and the "What's that floating in the water?" lines play.
2. **Click the bottle** in the water. A line plays (*You fish the bottle out...*), then the **note opens on a piece of paper** covered in strange symbols. Click or press Enter to put it away.
3. The player says a book might help. Task: *Find a book inside the lighthouse that can help read the message*.
4. On **Floor 3**, press E on the books. The decoys say "not this one". **Book - Strange Symbols** matches the symbols to numbers and gives the code **2 7 4 1**. Task: *Open the safe on the fourth floor (code: 2741)*.
5. On **Floor 4**, press E at the **Safe** to open its keypad. Type the digits or click the buttons; the code is checked once 4 digits are in. A wrong code shows "Wrong code" and clears. The right one shows *Click*, the safe swings open, and **the last journal page (page 3)** is inside.
6. Page 3 finishes Day 4's puzzle. "Head to the top" plays, and going up starts the **last night: 7 ships, 75 seconds**.
7. After the night report: fade, then the Day 4 bedtime card (*"We will pick you up bright and early in the morning."*). In the morning, **Ending 1 - Win** loads.

---

## Night time limit

| What | Where | Value |
|---|---|---|
| Seconds on night 1 | Night Scene → **Night Shift** → *Time Limit* → **First Night Seconds** | 30 |
| Seconds added each night | same → **Extra Seconds Per Night** | 15 (so 30 / 45 / 60 / 75) |
| The on-screen text | same → **Timer Text** (`{0}` = the time, e.g. 0:27) and **Time Up Text** | `TIME LEFT  {0}`, `TIME'S UP` |
| When it turns red | same → **Warning Seconds** / **Warning Color** | 10 s |
| Where it sits | Night Scene → **Binocular HUD → Night Timer** (a text object) | top-right corner |

How it works:
- The clock starts after the night's opening line. It pauses whenever the player is frozen (a dialogue or card).
- If every ship is reported in time, the night ends 2 s later, as before.
- If time runs out, the screen shows TIME'S UP and the player is frozen for 2 s (**End Delay**), so nothing more can be reported. Then the night report appears.
- **Ships nobody reported don't count:** they aren't guided, caught or missed. The report's new last line, *Not reported in time*, counts them.

Code: `Assets/Scripts & Controllers/Night/NightShift.cs`:
- `TimeLimit(night)` works out the seconds.
- `RunShift()` runs the clock.
- `TimeUp()` handles time running out.

`GameState.cs` gained `ShipsNotReported` / `RecordShipNotReported()`. The report text is in `DayManager.FinishNightShift()`; your "Abnormal ships…" wording was kept and one line was added.

---

## The message in a bottle

The bottle is `Outside` → **Interactables/Message in a Bottle**. Its old **Interact Dialogue** was replaced by **Message In A Bottle**, which is what `MessageInaBottle.cs` (an empty stub until now) does. It still has **Click To Interact** and **Show When Available**.

| Field | What it is |
|---|---|
| Prompt Text, Active On Days (4), Hidden By Flags (BottleMessage) | Same as before: it only floats there on Day 4, until it's read |
| **Paper Note Prefab** | `Assets/Prefabs/UI/Paper Note`. If it's ever empty, clicking the bottle logs an error and skips the paper (Read Dialogue and After Reading still run), so the quest can't get stuck |
| **Note Text** | What's written on the paper. Right now it's placeholder symbols (Greek/Cyrillic letters) |
| **Note Image** | Optional. Drop a sprite here (e.g. a drawing of the note) and it shows on the paper above the text |
| **Open Dialogue** | Plays before the paper opens (*You fish the bottle out...*) |
| **Read Dialogue** | Plays after the paper is put away |
| **After Reading** | Set Flag `BottleMessage`, New Task `day3_find_book` |

**Paper Note** (`Assets/Prefabs/UI/Paper Note.prefab`, script `UI/PaperNote.cs`) is a reusable full-screen paper with its own canvas (sorting 150). Any script can show a note:

```csharp
Instantiate(paperNotePrefab).Open(text, sprite, onClosed);
```

It closes on click or Enter (the `Click` and `Dialogue` actions). To restyle the paper, open the prefab and change **Paper** (colour/size), **Note Text** (font size/colour) or **Hint**.

## The book
The books now live on **`Floor 2`** (moved there with the bookshelf art; they're clicked). When they moved, this book's text and task went back to the old version, so they were set again. `Floor 2` → **Interactables/Book - Strange Symbols** (Interact Dialogue): the dialogue now gives the code. After the dialogue it sets `BottleDeciphered` and the task `day3_open_safe`, *Open the safe on the fourth floor (code: 2741)*. The three other books are unchanged decoys.

## The safe
`Floor 4` → **Safe**, at x −3.5 on the floor between the two staircases. It is new and uses `Safe.cs` (an empty stub until now):

| Field | Value |
|---|---|
| Prompt Text | Open the safe |
| Hidden By Flags | `SafeOpened`, so it can't be used again once open |
| **Lock → Unlock Flag** | `BottleDeciphered`. Until the book is read, E only says *"A heavy safe with a keypad. I don't know the code."* |
| **Code** | `2741` |
| Keypad Prefab | `Assets/Prefabs/UI/Safe Keypad` |
| Page Number | 3, the last journal page, which finishes Day 4's puzzle |
| Opened Dialogue | *The safe clicks open.* / *Inside is the last page of the journal.* / a placeholder player line |
| After Opened | Set Flag `SafeOpened` |
| Closed Art / Open Art | The two child objects **Closed** (grey box + keypad) and **Open** (dark inside + swung door). They're placeholder squares: replace their sprites with real art |

It also has a Box Collider 2D trigger (1.8 × 1.6) so the E prompt shows when you stand at it, and a **Label** copied from the key's.

**To change the code, change it in both places:**
1. **Safe → Code**
2. **Book - Strange Symbols** → its 2nd dialogue line, and its **New Task Text**

**Safe Keypad** (`Assets/Prefabs/UI/Safe Keypad.prefab`, script `Puzzles/SafeKeypad.cs`) has its own canvas (sorting 150):
- **Buttons:** each digit button's **On Click** calls `SafeKeypad.PressDigit` with its digit; **C** calls `ClearDigits`, **<** calls `DeleteDigit`, **X** closes.
- **Keyboard:** digit keys type; Backspace deletes; Esc steps away.
- **Code length:** it checks as soon as as many digits as the code has are typed, so a longer code just works.
- **Texts:** Instructions / Wrong Text / Correct Text are on the keypad component.

---

## The last night
Night Scene → **Ships** has three new holders, built like Fishing Ship 1 (same model, bob, outline). They have **Active On Nights = 4**:

| Object | Ship Name | Ghost? | Where |
|---|---|---|---|
| Fishing Ship 5 | Northern Star | no | due east, ~295 m. The console blocks the east window, so look from the NE or SE windows |
| Fishing Ship 6 | Kittiwake | no | north-north-west, ~350 m |
| Fishing Ship 7 | The Drowned Lantern | **yes** (dark outline) | north-east, ~410 m |

Ships per night are now **3 / 4 / 4 / 7**: the last night has 3 more than night 3. To add a ship to a night, duplicate a ship holder, move and turn the **holder**, and set its **Night Ship** values and **Active On Nights**. Each Day asset's **Ship Count** was set to match (3 / 4 / 4 / 7). It's only used in text.

## The ending
On the Systems prefab → **Day Manager → Ending**:
- **Ending Cutscene** was emptied. It was a placeholder card: *Ending / The player watches a ship arrive. A boat horn sounds.*
- **Victory Cutscene → Scene** = `Ending 1 - Win`.
- **Death Cutscene** is unchanged. It still plays instead of Victory if the player reported **Ghost Ships To Lose** (3) or more abnormal ships as Real.

So after Day 4's bedtime card, the morning loads **Ending 1 - Win** (it's in Build Settings).

**The ending dialogue:** `Ending 1 - Win` → **Story/Ending Dialogue** is a Story Trigger (When In Scene) that plays the Manager's two lines over the painting as soon as the scene opens:
- *"Congrats! You made it through the 4 days!"*
- *"I knew you'd be able to do it yourself! I hope there weren't too many issues along the way.."*

The lines were copied from `Assets/Data/Days/Win Cutscene.asset`. To change them, edit the trigger's **Dialogue**; the asset isn't used by anything (don't add it to Day Manager's Days list, or it becomes a 5th day). The dialogue box draws over the painting because the Systems UI canvas sorts at 100 and the ending's canvas at 0.

Its **Ends Cutscene** is off, so the game stays on the painting after the lines. Tick it to go back to the Title Screen instead: Day Manager then resets the game and loads its **Title Scene**.

**Ending 1 - Win** and **Cut Scene 1 - Intro** each had their own **EventSystem**. With the Systems prefab's EventSystem, that's two, and Unity printed *"There are 2 event systems in the scene"* every frame. Both are now switched off (unticked, not deleted). New scenes shouldn't get one: the Systems prefab always provides it.

---

## Day 1 again
A merge had emptied the `Day1` asset's **Puzzle Task Id**, **Puzzle Task Text** and **Required Flags**, so they were set back to:
- **Puzzle Task Id:** `day1_explore`
- **Puzzle Task Text:** `*Explore the house*`
- **Required Flags:** `ExploredFloor1`–`4`

Your edited wake line was kept. Everything else from the day reorder (Days 2–4 and the scene objects) had survived.

## Testing it
Set **Start On Day** to `4` on the Systems prefab, press Play in `Outside`, and play Day 4 through to the ending. Set it back to `1` afterwards.

What I tested:
- Day 1's task is "Explore the house".
- Night 1 with nothing reported: the timer counts down from 0:30, turns red, shows TIME'S UP, and the report says "Not reported in time: 3".
- Day 4 from start to finish: the bottle, the note, the book (code 2741), and the safe (a wrong code, then 2741, page 3, safe open).
- The last night: 7 ships, 1:15 on the clock, and the timer top-right clear of the task box. Then the report, the bedtime card, and **Ending 1 - Win** showing your painting.
- Real keyboard typing and the keypad buttons.

Not tested: real mouse clicks and real Enter presses on the bottle and the paper note. They were driven from code (clicks over Unity's MCP connection don't reach the game), so try them by hand once.
