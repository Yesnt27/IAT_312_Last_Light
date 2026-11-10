# Day 4 quest, night timer and ending

This covers what was built for the end of the game, and where to change each part yourself:
- the night time limit
- the message in a bottle → book → safe quest (Day 4)
- the bigger last night
- the ending scene

It also covers re-applying the Day 1 settings that a merge had emptied.

## The whole Day 4, in order
1. Wake up in the Floor 4 bed to the horn. Go outside, and the "What's that floating in the water?" lines play.
2. **Click the bottle** in the water. A line plays (*You fish the bottle out...*), then the **note opens on a piece of paper**: four strange symbols, `Ж Ω Ψ Δ`. Click to put it away. The note is now in the **inventory** (right of the screen), and F reads it again at any time.
3. The player says a book might help. Task: *Find a book inside the lighthouse that can help read the message*.
4. On **Floor 2**, click the **bookshelf**. The shelf opens full-screen with five books. Click a spine to open that book; *Put the book back* (or Esc) returns to the shelf. With **Strange Symbols** open, press **F** with the note selected to hold it up beside the book: *"The symbols on the note match the ones in this book!"* The book's table turns the note into **2 7 4 1**, but the game never says the number. Closing the shelf plays two lines, sets `BottleDeciphered` and the task *Decode the note with the symbol book, then open the safe on the fourth floor*.
5. On **Floor 4**, click the **Safe** (it lights up on hover, like the electrical box) to open its keypad. Type the digits or click the buttons; the code is checked once 4 digits are in. A wrong code shows "Wrong code" and clears. The right one shows *Click*, the safe swings open, and **the last journal page (page 4)** is inside.
6. Page 4 finishes Day 4's puzzle. "Head to the top" plays, and going up starts the **last night: 7 ships, 75 seconds**.
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
| **Note Item** | `Assets/Data/Items/Bottle Note`. The note's text and picture now come from this inventory item, so the paper here, the inventory copy and the one held up at the bookshelf always match. Edit the note on that asset |
| Note Text / Note Image | Only used if Note Item is empty. Note Text still holds the old placeholder symbols |
| **Open Dialogue** | Plays before the paper opens (*You fish the bottle out...*) |
| **Read Dialogue** | Plays after the paper is put away |
| **After Reading** | Set Flag `BottleMessage`, New Task `day3_find_book` |

**Paper Note** (`Assets/Prefabs/UI/Paper Note.prefab`, script `UI/PaperNote.cs`) is a reusable full-screen paper with its own canvas (sorting 150). Any script can show a note:

```csharp
Instantiate(paperNotePrefab).Open(text, sprite, onClosed);
```

It closes on a left click (the `Click` and `Dialogue` actions, both on the left mouse button). To restyle the paper, open the prefab and change **Paper** (colour/size), **Note Text** (font size/colour) or **Hint**.

## The bookshelf
`Floor 2` → **Interactables/Bookshelf** now has **Bookshelf** (`Bookshelf.cs`) + **Click To Interact**. Its clickable area is its placeholder **Art** square over the shelf. The four separate book objects (`Book - Fishing Almanac`, `Weather Logs`, `Strange Symbols`, `Ship Registry`) are **switched off, not deleted**; their lines were reused as book pages.

| Field | Value |
|---|---|
| Prompt Text | Search the bookshelf |
| Active On Days / Required Flags / Hidden By Flags | 4 / `BottleMessage` / `SafeOpened`. So it opens once the note is read, and stays open for a second look until the safe is open |
| Puzzle Prefab | `Assets/Prefabs/UI/Bookshelf Puzzle` |
| **Books** | One entry per book: **Title** (on the spine and the left page), **Spine Color**, **Left Page**, **Right Page**, **Is Clue**. Add, remove or reorder books here; the shelf builds the spines from this list |
| Clue Item | `Assets/Data/Items/Bottle Note`: what must be held up next to the clue book |
| Clue Found Dialogue | Two player lines, played after the shelf closes (only the first time) |
| After Finding Clue | Task `day3_open_safe`, *Decode the note with the symbol book, then open the safe on the fourth floor*; Set Flag `BottleDeciphered` (unlocks the safe) |

**Bookshelf Puzzle** (`Assets/Prefabs/UI/Bookshelf Puzzle.prefab`, script `Puzzles/BookshelfPuzzle.cs`) is the close-up: its own canvas (sorting 150, scaler *Expand*, so it fits any window shape), a dark Dim, the **Shelf** with a **Spine Row** (spines are copies of the hidden **Spine Template**), the **Open Book** (two pages + *Put the book back*), the **Held Note** paper on the right, the red X, a status line at the top and the item hint at the bottom. All its texts (instructions, *That won't help with this.*, the match message) are on the component. While it's open it takes over F from the inventory: a readable item is held up beside the book (F again puts it away); anything else gets *That won't help with this.* It's solved when the **Is Clue** book is open and the **Clue Item** is held up, in either order.

### The cipher
The note says `Ж Ω Ψ Δ`. The Strange Symbols book's right page is the key:

| Θ | Δ | Ж | Π | Ψ | Σ | Ξ | Ω | Φ | λ |
|---|---|---|---|---|---|---|---|---|---|
| 0 | 1 | 2 | 3 | 4 | 5 | 6 | 7 | 8 | 9 |

So the note reads 2 7 4 1. These letters come from the TextMesh Pro fallback font; pick others from the same Greek/Cyrillic range if you change them.

## The inventory
Slots on the right of the screen (Systems prefab → **UI Canvas/Inventory HUD**): at least 2 boxes, one per item carried (up to 4), the selected one outlined in yellow with its name and `[Scroll] Switch   [F] Use` under it. It hides in scenes without the 2D player (title, cutscenes, the 3D night). **Mouse wheel** picks an item, **F** uses it (`UseItem` / `CycleItem` in the `Controls` action map).

- **Items** are assets in `Assets/Data/Items/` (*Create → Lighthouse → Inventory Item*): name, icon, **Held While Flag** (the player has it while that flag is set), optional **Gone When Flag**, and what F does: **Note Text / Note Image** make it readable (F opens it on the Paper Note), otherwise **Use Dialogue** plays.
- **The list** is Systems prefab → **Inventory → All Items**, in slot order: `Basement Key` (held while `BasementKey`), `Bottle Note` (held while `BottleMessage`).
- **To add an item:** create the asset, set its flag, add it to All Items, and set that flag when it's picked up (e.g. a FlagPickup or a StoryOutcome). Because items come from flags, a new game empties the inventory and Start On Day fills it with earlier days' items.
- Scripts: `Inventory/Inventory.cs` (on Systems), `Inventory/InventoryHud.cs`, `Inventory/InventoryItem.cs`, and `Inventory/IItemUseHandler.cs` (how a puzzle like the bookshelf takes over F while it's open).

## The safe
`Floor 4` → **Safe**, at x −3.5 on the floor between the two staircases. It is new and uses `Safe.cs` (an empty stub until now):

| Field | Value |
|---|---|
| Prompt Text | Open the safe |
| Hidden By Flags | `SafeOpened`, so it can't be used again once open |
| **Lock → Unlock Flag** | `BottleDeciphered`. Until the book is read, E only says *"A heavy safe with a keypad. I don't know the code."* |
| **Code** | `2741` |
| Keypad Prefab | `Assets/Prefabs/UI/Safe Keypad` |
| Page Number | 4, the last journal page, which finishes Day 4's puzzle |
| Opened Dialogue | *The safe clicks open.* / *Inside is the last page of the journal.* / a placeholder player line |
| After Opened | Set Flag `SafeOpened` |
| Closed Art / Open Art | **Closed/Safe_0** (the closed safe art) and **Closed/open safe_0** (the open safe art). Opening the safe hides the first and shows the second |

It has **Click To Interact**: the clickable area is its art, which tints yellow on hover, and E no longer does anything there. Its old Box Collider 2D trigger is still on it but unused. The art is under **Closed**: **Safe_0** (closed, the safe's Closed Art) and **open safe_0** (the Open Art).

**To change the code, change it in all three places:**
1. **Safe → Code**
2. **`Data/Items/Bottle Note` → Note Text**: the symbols for the new digits
3. **Floor 2 → Bookshelf → Books → Strange Symbols → Right Page**: only if you change which symbol means which digit

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
- Day 4 from start to finish: the bottle, the note, the book (code 2741), and the safe (a wrong code, then 2741, the last page, safe open).
- The last night: 7 ships, 1:15 on the clock, and the timer top-right clear of the task box. Then the report, the bedtime card, and **Ending 1 - Win** showing your painting.
- Real keyboard typing and the keypad buttons.

Not tested: real mouse clicks and real Enter presses on the bottle and the paper note. They were driven from code (clicks over Unity's MCP connection don't reach the game), so try them by hand once.
