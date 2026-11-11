# Day 4 quest, night timer and ending

This covers what was built for the end of the game, and where to change each part yourself:
- the night time limit
- the message in a bottle → book → safe quest (Day 4)
- the bigger last night
- the ending scene

It also covers re-applying the Day 1 settings that a merge had emptied.

## The whole Day 4, in order
1. Wake up in the Floor 4 bed to the horn. Go outside, and the "What's that floating in the water?" lines play.
2. **Click the bottle** in the water. A line plays (*You fish the bottle out...*), then the **note opens on a piece of paper**: six picture symbols (`symbols_paper`: ship, lighthouse, fish, bird, anchor, lighthouse). Click to put it away. The note is now in the **inventory** (right of the screen), and F reads it again at any time.
3. The player says a book might help. Task: *Find a book inside the lighthouse that can help read the message*.
4. On **Floor 2**, click the **bookshelf**. The shelf opens full-screen with four books (Fishing Almanac, Sailor's Poems, Symbols, Ship Registry); each opens to its picture from `Journal Assets`. Click a spine to open that book; *Put the book back* (or Esc) returns to the shelf. The note is shown beside every open book by itself. Opening **Symbols** solves it: *"The symbols on the note match the ones in this book!"* The book's key turns the note into **7 4 8 5 2 4**, but the game never says the number. Closing the shelf plays two lines, sets `BottleDeciphered` and the task *Decode the note with the symbol book, then open the safe on the fourth floor*.
5. On **Floor 4**, click the **Safe** (it lights up on hover, like the electrical box) to open its keypad. Type the digits or click the buttons; the code is checked once all 6 digits are in. A wrong code shows "Wrong code" and clears. The right one shows *Click*, the safe swings open, and **the last journal page (page 4)** is inside.
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
| **Note Item** | `Assets/Data/Items/Bottle Note`. The note's text and picture now come from this inventory item, so the paper here, the inventory copy and the one shown beside the books at the bookshelf always match. Edit the note on that asset |
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
`Floor 2` → **Interactables/Bookshelf** now has **Bookshelf** (`Bookshelf.cs`) + **Click To Interact**. Its clickable area is its placeholder **Art** square over the shelf. The four old book objects (`Book - Fishing Almanac`, `Weather Logs`, `Strange Symbols`, `Ship Registry`), switched off since the bookshelf came in, have been **deleted**.

| Field | Value |
|---|---|
| Prompt Text | Search the bookshelf |
| Active On Days / Required Flags / Hidden By Flags | 4 / `BottleMessage` / `SafeOpened`. So it opens once the note is read, and stays open for a second look until the safe is open |
| Puzzle Prefab | `Assets/Prefabs/UI/Bookshelf Puzzle` |
| **Books** | One entry per book: **Title** (on the spine), **Spine Color**, **Open Pages** (the open-book picture, e.g. `Fishing_Almanac`; import it as Sprite (2D and UI)), **Is Clue** (only `Symbols`). Add, remove or reorder books here; the shelf builds the spines from this list |
| Clue Item | `Assets/Data/Items/Bottle Note`: the note shown beside every open book; opening the clue book while carrying it solves the puzzle |
| Clue Found Dialogue | Two player lines, played after the shelf closes (only the first time) |
| After Finding Clue | Task `day3_open_safe`, *Decode the note with the symbol book, then open the safe on the fourth floor*; Set Flag `BottleDeciphered` (unlocks the safe) |

**Bookshelf Puzzle** (`Assets/Prefabs/UI/Bookshelf Puzzle.prefab`, script `Puzzles/BookshelfPuzzle.cs`) is the close-up: its own canvas (sorting 150, scaler *Expand*, so it fits any window shape), a dark Dim, the **Shelf** with a **Spine Row** (spines are copies of the hidden **Spine Template**), the **Open Book** (two pages + *Put the book back*), the **Held Note** paper on the right, the red X and a status line at the top. All its texts (instructions, *Compare the book with the note*, the match message) are on the component.

- **Hover:** spines tint yellow under the mouse, like the electrical box (**Hover Tint**, default (1, 1, 0.55)), and the book's **title appears only while the mouse is over its spine**. That's the **Hover Reveal** component on the Spine Template, whose Target is the spine's Label.
- **The books** (Floor 2 → Bookshelf → Books), in shelf order: Fishing Almanac, Sailor's Poems, Supernatural Creatures, **Symbols** (the clue), Grocery List and Ship Registry. Each one opens to its picture in `Visual Assets/Journal Assets`.
- **The note shows by itself:** if the player carries the **Clue Item**, its paper appears beside every open book. There's no F step any more, and the old item hint at the bottom was removed.
- **Solving:** opening the **Is Clue** book while carrying the Clue Item solves it.

### The cipher
The note is the picture `Journal Assets/symbols_paper`: ship, lighthouse, fish, bird, anchor, lighthouse. The **Symbols** book picture is the key:

| dog | anchor | spade | lighthouse | bird | cat | ship | fish | club | heart |
|---|---|---|---|---|---|---|---|---|---|
| 1 | 2 | 3 | 4 | 5 | 6 | 7 | 8 | 9 | 10 |

So the note reads **7 4 8 5 2 4**, the Safe's Code. The bottle note item has no text, only this picture; `UI/NoteDisplay` draws a picture-only note as the whole paper (on the Paper Note and beside the books).

## The inventory
Slots on the right of the screen (Systems prefab → **UI Canvas/Inventory HUD**): at least 2 boxes, one per item carried (up to 4), the selected one outlined in yellow with its name and `[1-2] Switch   [F] Use` under it (just `[F] Use` while you carry one item). Each slot has its key number to its left. It hides in scenes without the 2D player (title, cutscenes, the 3D night). The **number keys 1–4** pick the item in that slot (keys past the last item do nothing), **F** uses it. Both are actions in the `Controls` map: `SelectSlot` (one binding per slot, in slot order; rebind there) and `UseItem`. The mouse wheel no longer switches items.

- **Items** are assets in `Assets/Data/Items/` (*Create → Lighthouse → Inventory Item*): name, icon, **Held While Flag** (the player has it while that flag is set), optional **Gone When Flag**, and what F does: **Note Text / Note Image** make it readable (F opens it on the Paper Note), otherwise **Use Dialogue** plays.
- **The list** is Systems prefab → **Inventory → All Items**, in slot order: `Basement Key` (held while `BasementKey`), `Bottle Note` (held while `BottleMessage`; its icon is the `symbols_paper` picture, the same as the note itself).
- **To add an item:** create the asset, set its flag, add it to All Items, and set that flag when it's picked up (e.g. a FlagPickup or a StoryOutcome). Because items come from flags, a new game empties the inventory and Start On Day fills it with earlier days' items.
- Scripts: `Inventory/Inventory.cs` (on Systems), `Inventory/InventoryHud.cs`, `Inventory/InventoryItem.cs`, and `Inventory/IItemUseHandler.cs` (how a puzzle can take over F while it's open; nothing uses it at the moment).

## The safe
`Floor 4` → **Safe**, at x −3.5 on the floor between the two staircases. It is new and uses `Safe.cs` (an empty stub until now):

| Field | Value |
|---|---|
| Prompt Text | Open the safe |
| Hidden By Flags | `SafeOpened`, so it can't be used again once open |
| **Lock → Unlock Flag** | `BottleDeciphered`. Until the book is read, E only says *"A heavy safe with a keypad. I don't know the code."* |
| **Code** | `748524` (from the note and the Symbols book) |
| Keypad Prefab | `Assets/Prefabs/UI/Safe Keypad` |
| Page Number | 4, the last journal page, which finishes Day 4's puzzle |
| Opened Dialogue | *The safe clicks open.* / *Inside is the last page of the journal.* / a placeholder player line |
| After Opened | Set Flag `SafeOpened` |
| Closed Art / Open Art | **Closed/Safe_0** (the closed safe, `Backgrounds and Furnish/Safe.png`) and **Closed/Safe_open** (the open safe, `Safe_open.png`). Opening the safe hides the first and shows the second, and it stays open on later visits (once page 4 is collected) |

It has **Click To Interact**: the clickable area is its art, which tints yellow on hover, and E no longer does anything there. Its old Box Collider 2D trigger is still on it but unused. The art is under **Closed**: **Safe_0** (Closed Art) and **Safe_open** (Open Art). Both pictures use 798 pixels per unit, so the closed safe is 2.38 units wide, and both stand on the cabinet top (bottom at y 0.535). The open picture's safe body is its right part (x 1022–2877 of 2899 px), placed exactly over the closed safe, with the door swinging out to the left. `Safe.png` keeps its one slice `Safe_0` (widened to the whole 1899 × 1517 picture when it was redrawn), so the scene's reference didn't change. The old `open safe.png` is no longer used.

**To change the code, change it in all three places:**
1. **Safe → Code**
2. **The note picture** (`symbols_paper`, set as `Data/Items/Bottle Note` → Note Image): the symbols for the new digits
3. **The Symbols book picture** (`Floor 2 → Bookshelf → Books → Symbols → Open Pages`): only if you change which symbol means which digit

The keypad works with any code length. The clue-found line on the Bookshelf says "six-digit lock", so change it too if the length changes.

**Safe Keypad** (`Assets/Prefabs/UI/Safe Keypad.prefab`, script `Puzzles/SafeKeypad.cs`) has its own canvas (sorting 150):
- **Art:** **Panel** shows `Backgrounds and Furnish/Safe_keypad.png` at 1.3× (588 × 819). Its children are anchored to the drawing in proportion, so resizing Panel keeps everything lined up. **Screen** covers the drawn screen (transparent): **Display** (the digits, auto-sized) on its top part and **Status** (*Enter the code* / *Wrong code* / *Click*) underneath. **Title** (*SAFE*) sits above the keypad, and the **X** is on its top-right corner.
- **Keys:** the 12 buttons (1–9, C, 0, <) sit over the drawn keys. Their own image is invisible; hovering lights a key yellow (like hovering the safe) and pressing darkens it (each button's colours). The dark labels name the keys, because the drawn keys are blank.
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
- Day 4 from start to finish: the bottle, the note, the book (code 2741 at the time; now 748524), and the safe (a wrong code, then the code, the last page, safe open).
- The last night: 7 ships, 1:15 on the clock, and the timer top-right clear of the task box. Then the report, the bedtime card, and **Ending 1 - Win** showing your painting.
- Real keyboard typing and the keypad buttons.

Not tested: real mouse clicks and real Enter presses on the bottle and the paper note. They were driven from code (clicks over Unity's MCP connection don't reach the game), so try them by hand once.
