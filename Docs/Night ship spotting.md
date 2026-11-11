# Night ship spotting

How the night shift works after the rework, which ship is which on each night, and how to change them.

## How a night plays

1. The night starts with its opening line. The clock shows the full time but **doesn't run yet**. The task says *Pick up the binoculars on the desk to start your watch*.
2. **Picking up the binoculars** (look at them + E) starts the clock.
   - Nights last 60 / 75 / 90 / **180** s: Night Shift → **Night Seconds** (element 0 = night 1; nights past the list use its last entry). Night 4 has nine ships, so it gets 3 minutes.
3. **Sit in the chair:** look at it + E. You can look all around and use the binoculars, but you can't walk. **Space** stands you up; the game doesn't say so.
   - **It's a swivel chair.** It turns with you, so its backrest is always behind you and never blocks the view.
   - **Seated view:** your eyes are high up, at y 10.0 (1.45 m above standing height), so you see over the desk console and the window sill all round.
   - **To change the seated height:** move `Night Controls/Chair Seat/Eye Point` up or down (Position Y). Keep X and Z as they are; that's the chair's stem, which it swivels around.
   - **Standing up:** the chair swings back to face the windows.
4. **Right click** to raise the binoculars (right click again lowers them), wait for the lighthouse beam to light a ship, then **left click** to select it (green outline).
   - The buttons are the `Use` (right mouse) and `Select` (left mouse) actions in the `FirstPerson` map; swap them back there if needed, and change the three hint texts to match (`Binocular HUD` → Aim Text, the `Held Hint` and `Lower Hint` objects).
5. **Report it from the chair:**
   - **Q** = Report (it's a ghost), **E** = Safe.
   - This works even with the binoculars still up; the binocular HUD shows `[Q] Report   [E] Safe` under `SHIP SELECTED`.
   - Standing, you can still walk to the buttons on the desk and press Q/E while looking at them.
6. **Forgot what to look for?** Look at the **clipboard** on the desk (right of the binoculars, next to the weather map) and press **F**. The journal opens at the newest page; the arrow keys go back to earlier nights' notes, and **Esc** (or a click) puts it down. It works from the chair too. The clock pauses while the journal is open.
7. When every ship is reported, or the time runs out, the night report plays and you're back at the bed on Floor 4.

Nothing on screen tells you which ships are ghosts. Ghost ships used to have a dark outline; that's gone. The tells are in the models, and each journal entry's **Night Notes** says what to look for.

## The ships on each night

All in the Night Scene under **Ships**. Each `Ship N` matches your per-day ship sheet.

| Night | Ship | Model | Ghost? | Where (from the chair) |
|---|---|---|---|---|
| 1 | Ship 1 | D1_Ship1 | no | west, 285 m |
| 1 | Ship 2 | D1_Ship2 | **yes** | north-west, 300 m |
| 1 | Ship 3 | D1_Ship3 | **yes** | south-west, 290 m |
| 2 | Ship 1 | **D1_Ship1 (stand-in)** | no | south-west, 285 m |
| 2 | Ship 2 | D2_Ship2 | no | west, 275 m |
| 2 | Ship 3 | D2_Ship3 | no | north, 300 m |
| 2 | Ship 4 | D2_Ship4 | **yes** | north-west, 300 m |
| 2 | Ship 5 | D2_Ship5 | **yes** | west-south-west, 325 m |
| 3 | Ship 1 | D3_Ship1 | no | west-south-west, 325 m |
| 3 | Ship 2 | D3_Ship2 | no | north-west, 320 m |
| 3 | Ship 3 | D3_Ship3 | **yes** | south-west, 280 m |
| 3 | Ship 4 | D3_Ship4 | **yes** | west, 275 m |
| 3 | Ship 5 | D3_Ship5 | **yes** | west, 305 m |
| 3 | Ship 6 | D3_Ship6 | **yes** | north-west, 285 m |
| 3 | Ship 7 | D3_Ship7 | **yes** | north, 300 m |
| 4 | Ship 1 | D4_Ship1 | no (no figures, right symbol, 2 valves, bright) | west, 300 m |
| 4 | Ship 2 | D4_Ship2 | no (normal figures, 4 valves, bright) | south-west, 300 m |
| 4 | Ship 3 | D4_Ship3 | no (normal figures, right symbol, code LAW030) | west, 300 m |
| 4 | Ship 4 | D4_Ship4 | no (no figures, right symbol, code PER011) | north-west, 300 m |
| 4 | Ship 5 | D4_Ship5 | **yes** (flickering figures) | south-west, 280 m |
| 4 | Ship 6 | D4_Ship6 | **yes** (flickering figures, symbol the wrong way) | north-west, 285 m |
| 4 | Ship 7 | D4_Ship7 | **yes** (flickering figures, wrong code 39AKJD, valves) | south-west, 280 m |
| 4 | Ship 8 | D4_Ship8 | **yes** (dull colour) | west, 280 m |
| 4 | Ship 9 | D4_Ship9 | **yes** (everything wrong, code EF270A) | north-west, 285 m |

- **Night 2's Ship 1 is a stand-in.** There's no `D2_Ship1` model yet, so it uses D1_Ship1 (bright red, 2 valves). That still counts as a normal ship by the rules.
- **Night 4** (`Ships/Night 4`) replaced the seven old fishing ships, which are deleted. Its nine ships sit three to each west window pane, ghosts and normals mixed, all fully visible from the chair. They show their **port side** to the lighthouse, because the D4 models carry the anchor (the symbol) on the port bow.
- **Flickering figures:** Night 4's ghosts 5, 6, 7 and 9 have **Flicker Figures** on the holder. Its **Figures** list holds the person meshes of the model; every 0.6–2 s they blink out for 0.06–0.18 s, one to three times. Ship 8's only tell is its dull colour, so it has none. To make a figure flicker on another ship, add the component to the holder and drag the figure's mesh object into Figures.
- **Ship height:** the holders sit higher than the sea (y −25): Night 1 at y −13, Nights 2–4 at y −16. Night 4 copies Nights 2–3.
- **Hull colours:** a few models aren't the colour your sheet says (Night 2's Ship 2 is bright blue, not orange). Both are bright, so the answer doesn't change.

## Changing things

- **Swap a model** (for example when D2_Ship1 is ready): open `Ships/Night 2/Ship 1 (stand-in model)`, delete its model child, drag the new `.glb` onto the holder, and set the child's Position to (0, −1, 0). Leave the child's rotation as it comes, because some models carry their own rotation.
- **Ghost or normal:** the holder's **Night Ship → Is Ghost**.
- **Which night:** **Night Ship → Active On Nights** (the day number). The Day asset's **Ship Count** should match the number of ships that night; it's only used in text.
- **Move a ship:** move or turn the **holder**, not the model. Keep it inside a window you can see from the chair:
  - **Clear:** the west panes, bearings −153…−115, −107…−70 and −62…−25, plus a narrow north pane, 4…18. The window posts in between (≈ −157, −111, −66, −22 and +21, about 7° each) hide a ship completely. A ship at ~290 m needs about 9° of glass.
  - **Blocked:** the desk covers everything east, and the island hides the lower hull to the south.
  - **Distance and angle:** 275–325 m out and side-on to the lighthouse lets you read codes and count valves through the binoculars.
- **Try a night without playing the whole day:** set **Night Shift → Preview Night** to 1, 2, 3 or 4 and press Play in the Night Scene. There's no clock in this mode.

## Code

| File | What changed |
|---|---|
| `Night/NightShip.cs` | The ghost outline is gone; only the selected (green) outline remains |
| `Night/NightShift.cs` | Waits for the first binocular pick-up before the clock runs; **Preview Night** for direct Play |
| `Night/ShipReportPanel.cs` | Seated: Q/E report the selected ship from the chair (even while looking through the binoculars). Standing: look at the buttons as before |
| `Night/BinocularHud.cs` | Shows the report keys under `SHIP SELECTED` when you can report from where you sit |
| `First Person/FirstPersonController.cs` | `SitAt` / `StandUp`; Space stands up while seated |
| `Night/FlickerFigures.cs` | New: blinks a ghost ship's figures (Night 4's tell) |
| `Night/NotesClipboard.cs` | New: the clipboard on the desk. Look at it + F to read the journal |
| `Night/NightShift.cs` (again) | **Night Seconds** list instead of First Night Seconds + Extra Seconds Per Night; Night 4 = 180 s |
| `Journal.cs`, `UI/JournalView.cs` | `Journal.OpenBook()` opens the book from anywhere; the journal now also closes on **Esc** |
| `First Person/Seat.cs` | New: the chair. Look at it + E to sit. Its **Chair** model swivels with you around **Eye Point** (keep Eye Point on the chair's stem) and turns back to **Chair Facing** when you stand |
