# Journal pages

The old keeper's journal pages now open on the same paper as the message in a bottle (`Assets/Prefabs/UI/Paper Note`). Every page collected so far can be read again at any time with **R**.

## Where the page text goes

All of it is in one place: **Systems prefab** (`Assets/Prefabs/Resources/Systems.prefab`) → **Journal** → **Pages**.

- **Element 0** is page 1 (Journal Entry 1), **Element 1** is page 2, and so on.
- Each one is a multi-line text box. Blank lines make paragraph gaps.
- The text shrinks to fit the paper, down to size 22, so a long page still fits.
- To add a page 5, raise the list's size to 5 and write Element 4.
- An empty element shows just "Page N", so a missing text is obvious.

Right now it holds Journal Entries 1–4. Each has **Diary:** and **Features: / Night N Notes:**, which are the clues for spotting that night's ghost ships.

**The tags in the text:**
- `<align=left>` at the start of each entry left-aligns the journal pages. The bottle's note stays centred.
- `<b>…</b>` makes the headings bold.
- Keep the tags when you edit; you can use `<i>…</i>` too.

## How pages are found

There's one page per day: entry N is found on Day N, before night N.

| Page | Day | Where | What happens |
|---|---|---|---|
| 1 | 1 | Floor 2 → **Interactables/Journal** (click it) | *"You found a journal…"* + the player's line, then page 1 opens |
| 2 | 2 | Basement → **Interactables/Journal Page 2**, beside the food (click it) | *"A journal page is tucked between the cans."*, then page 2 opens |
| 3 | 3 | Basement → **Electrical Box** | *"A journal page is taped to the inside of the door"* + the cut-wires line, then page 3 opens, then the wire puzzle |
| 4 | 4 | Floor 4 → **Safe** (after the right code) | *"The safe clicks open…"* lines, then page 4 opens |

In each case the finding line plays first, then the paper opens. Click to put it away.

**Picked up by walking into it:** Floor 2's Journal has a trigger **Box Collider 2D** and **Found Page → Pick Up On Touch** ticked. Walking onto it picks it up, and clicking still works. To do the same for another page, add a Box Collider 2D with **Is Trigger** ticked and tick Pick Up On Touch. Without Is Trigger, the collider blocks the player instead.

**Each day's page is required.** A day's puzzle only ends once that day's page is collected: each Day asset's **Journal Page** = its day number. Day 2 now needs page 2 + `FoodFound` + `BasementKey`.

**Where the page number is set:**
- Floor 2's Journal and Journal Page 2: **Page Number** on their Found Page.
- Electrical Box: **Page Number** (3).
- Safe: **Page Number** (4).

If you move a page to another day, change that number and the Day asset's **Journal Page** together.

**Changing the finding lines:**
- Floor 2's Journal and Journal Page 2: **Found Dialogue**.
- Electrical Box: **Page Found Dialogue**.
- Safe: **Opened Dialogue**.

**A new page somewhere else:** use the `Journal Page` prefab (`Assets/Prefabs/Interactables/Journal Page.prefab`). Set its **Page Number** and **Active On Days**, then write the text in Journal → Pages.

## Reading them again

- The journal icon (`Assets/Visual Assets/journal.png`) in the **bottom-right corner**, with `[R] Journal` under it, appears once at least one page is collected.
- It hides during dialogue and puzzles, and in scenes without the 2D player (title, cutscenes, the 3D night).
- **R** opens the journal at the newest page; **A / D** flip back and forward. A **click** or **R** again closes it.
- R does nothing while a dialogue, puzzle or other paper is open.
- The key is the `Journal` action in the `Controls` map; the on-screen hint follows its binding.

**The icon:** open the Systems prefab → `UI Canvas/Journal HUD`:
- **Book** is an Image showing `journal.png` at 120 × 150, with Preserve Aspect on. To use other art, import it with **Texture Type = Sprite (2D and UI)** and drag it into Book's **Source Image**. A plain texture won't go into a UI Image.
- **Label** is the `[R] Journal` hint text.
- **Journal HUD**'s Rect Transform is anchored to the bottom-right corner (Pos −36, 30). Change it to move both together.

## Code

| File | What changed |
|---|---|
| `Journal.cs` | Was an empty stub. Now the journal on the Systems prefab: page texts, the R key, the corner icon, and `Journal.Instance.ShowPage(page, onClosed)`, which opens the collected pages at that page |
| `UI/PaperNote.cs` | Can show several pages: `Open(pages, startPage, onClosed)`, with A/D to flip (the `Move` action) and the **Page Label** at the bottom of the paper. Single notes (the bottle) work as before. `Close()` is public |
| `FoundPage.cs`, `ElectricalBox.cs`, `Safe.cs` | After their dialogue, call `Journal.Instance.ShowPage(...)` before carrying on |
| `Day Flow/DayManager.cs` | The "head to the top" line now waits until the player is free (no dialogue, paper or puzzle open), so it can't start underneath a journal page |
