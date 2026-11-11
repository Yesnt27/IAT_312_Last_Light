# Journal pages

The old keeper's journal is drawn as a book. Each day's entry is a picture from `Assets/Visual Assets/Journal Assets/`. When you find a page you see it as a loose sheet of paper, and **R** opens the book at any time. The **arrow keys** turn its pages.

## Which picture is which

Everything is set in one place: **Systems prefab** (`Assets/Prefabs/Resources/Systems.prefab`) → **Journal** → **Pages**.

Element 0 is page 1 (found on Day 1), element 1 is page 2, and so on. Each element has two pictures:
- **Book Spread:** the open book with that day's entry. This is what the journal shows with R. The right-hand page has that night's ghost-ship notes.
- **Found Page:** the loose page as it's picked up. Leave it empty if the page isn't a loose sheet (Day 1, where you find the journal itself).

| Page | Day | Book Spread | Found Page |
|---|---|---|---|
| 1 | 1 | Day 1 Journal | (none: you find the book) |
| 2 | 2 | Day 2 Journal | Day 2 Pages |
| 3 | 3 | Day 3 Journal | Day 3 Pages |
| 4 | 4 | **(none yet)** | Day 4 Pages |

**A day with only one of the two pictures shows that one everywhere.** Day 4 has no book spread yet, so the book shows the Day 4 loose page.

**When the Day 4 spread is drawn:**
1. Import it into `Journal Assets` as **Texture Type = Sprite (2D and UI)**.
2. Drag it into Pages → Element 3 → **Book Spread**.

The Night 4 notes that used to be written in the text version ("figures appear on the ship, odd ones flicker…") aren't in any picture yet. Put them on that spread.

**To add a page 5:** raise the list's size to 5 and fill in Element 4.

**A page with no pictures at all** logs a warning ("Journal: page N has no art") and the game carries on without showing anything.

## What the player sees

**Finding a page** (Floor 2's journal, the Basement page, the electrical box, the safe):
1. The finding dialogue plays.
2. The **found page** fills the screen (Day 1: the book instead). A click puts it away.
3. The **book** then opens at that day's spread, so the day's notes are in front of the player. A click closes it and the story carries on.

Step 3 only happens when the book shows something different from step 2. For Day 4, which has only the loose page, it's skipped. To turn step 3 off for every page, untick **Journal → Open Book After Finding**.

**Reading it again:**
- **R** opens the book at the newest page collected so far.
- The **Left / Right arrow keys** turn back and forward through every collected page. The label at the bottom says `Page 2 / 3`; it's hidden when there's only one page.
- A **click** or **R** closes it.
- R does nothing while a dialogue, puzzle or other paper is open, or in scenes without the 2D player.
- The journal icon (`journal.png`, bottom-right, `[R] Journal`) appears once a page is collected. It hides during dialogue and puzzles.

## The pieces

| Piece | What it is |
|---|---|
| `Journal.cs` (Systems prefab) | The list of pages, the R key, the corner icon, and `Journal.Instance.ShowPage(page, onClosed)` for when a page is found |
| `JournalPage.cs` | One page's two pictures (Book Spread, Found Page) |
| `UI/JournalView.cs` + `Assets/Prefabs/UI/Journal View.prefab` | The full-screen picture: a dark backdrop, the **Page** image (Preserve Aspect, so a book spread and a tall loose page both fit), **Page Label** (arrow-key hint and page number) and **Hint** ("[Click] Close"). It has its own canvas (Sort Order 150), freezes the player while open, and destroys itself when closed |
| `TurnPage` action | In `PlayerControls.inputactions` → Controls: a 1D axis with Left Arrow (−) and Right Arrow (+). Rebind it there |

**To change the look:** open `Journal View.prefab`.
- **Page**'s Rect Transform margins set how big the pictures get. It's 140 px from the sides, 120 from the bottom and 40 from the top on a 1920 × 1080 screen.
- **Dim**'s colour sets how dark the game gets behind it.
- The hint text is on **Journal View → Page Text** and on the **Hint** object.

## Where pages are found

There's one page per day. Each day's puzzle only ends once that day's page is collected: each Day asset's **Journal Page** = its day number.

| Page | Where | Set its number on |
|---|---|---|
| 1 | Floor 2 → **Interactables/Journal** (click it or walk into it) | Found Page → **Page Number** |
| 2 | Basement → **Journal Page 2**, beside the food | Found Page → **Page Number** |
| 3 | Basement → **Electrical Box** (before the wire puzzle) | Electrical Box → **Page Number** |
| 4 | Floor 4 → **Safe** (after the right code) | Safe → **Page Number** |

If you move a page to another day, change that number and the Day asset's **Journal Page** together.

**Changing the lines said when a page is found:**
- Floor 2's Journal and Journal Page 2: **Found Dialogue**.
- Electrical Box: **Page Found Dialogue**.
- Safe: **Opened Dialogue**.

**A new page somewhere else:** use the `Journal Page` prefab (`Assets/Prefabs/Interactables/Journal Page.prefab`). Set its **Page Number** and **Active On Days**, then add its pictures in Journal → Pages.

**Picked up by walking into it:** Floor 2's Journal has a trigger **Box Collider 2D** and **Found Page → Pick Up On Touch** ticked. To do the same for another page, add a Box Collider 2D with **Is Trigger** ticked and tick Pick Up On Touch.

## What changed from the text version

- The text pages (Journal → Pages as text, shown on the Paper Note) are gone. The pictures replace them.
- `UI/PaperNote.cs` is back to a single note: the message in a bottle and readable inventory items. Its multi-page code, A/D flipping and **Page Label** were removed.
- `FoundPage`, `ElectricalBox` and `Safe` didn't change: they still call `Journal.Instance.ShowPage(...)` after their dialogue.
