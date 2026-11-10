# Night ship spotting

How the night shift works after the rework, which ship is which on each night, and how to change them.

## How a night plays

1. The night starts with its opening line. The clock shows the full time but **doesn't run yet**. The task says *Pick up the binoculars on the desk to start your watch*.
2. **Picking up the binoculars** (look at them + E) starts the clock.
   - Nights last 60 / 75 / 90 / 105 s (Night Shift → **First Night Seconds** 60 + **Extra Seconds Per Night** 15).
3. **Sit in the chair:** look at it + E. You can look all around and use the binoculars, but you can't walk. **Space** stands you up; the game doesn't say so.
4. **Click** to raise the binoculars, wait for the lighthouse beam to light a ship, then **right click** to select it (green outline).
5. **Report it from the chair:**
   - **Q** = Report (it's a ghost), **E** = Safe.
   - This works even with the binoculars still up; the binocular HUD shows `[Q] Report   [E] Safe` under `SHIP SELECTED`.
   - Standing, you can still walk to the buttons on the desk and press Q/E while looking at them.
6. When every ship is reported, or the time runs out, the night report plays and you're back at the bed on Floor 4.

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
| 4 | the 7 old fishing ships | fishing_ship | Hollow Bell and Drowned Lantern | as before |

- **Night 2's Ship 1 is a stand-in.** There's no `D2_Ship1` model yet, so it uses D1_Ship1 (bright red, 2 valves). That still counts as a normal ship by the rules.
- **Night 4** still uses the old fishing ships (`Ships/Night 4 (old ships)`) until its new models exist. Their **Details** text still describes them, because they have no other tells.
- **Hull colours:** a few models aren't the colour your sheet says (Night 2's Ship 2 is bright blue, not orange). Both are bright, so the answer doesn't change.

## Changing things

- **Swap a model** (for example when D2_Ship1 is ready): open `Ships/Night 2/Ship 1 (stand-in model)`, delete its model child, drag the new `.glb` onto the holder, and set the child's Position to (0, −1, 0). Leave the child's rotation as it comes, because some models carry their own rotation.
- **Ghost or normal:** the holder's **Night Ship → Is Ghost**.
- **Which night:** **Night Ship → Active On Nights** (the day number). The Day asset's **Ship Count** should match the number of ships that night; it's only used in text.
- **Move a ship:** move or turn the **holder**, not the model. Keep it inside a window you can see from the chair:
  - **Clear:** west to north-west, plus a narrow pane due north.
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
| `First Person/Seat.cs` | New: the chair. Look at it + E to sit |
