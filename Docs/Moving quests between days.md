# Moving quests between days

This guide records how the day order was changed, so you can make the same kind of change yourself. **The reorder needed no code**: everything was done in the Unity Inspector, by editing the Day assets and some objects in the story scenes. The only code added afterwards is the **Start On Day** testing option (see *Testing a later day*).

> **Later change:** the journal (page 1) moved to **Day 1**, on Floor 2's coffee table, so it's read before the first night. Day 1 now needs page 1 + `ExploredFloor1`–`4` + `Journal`; Day 2 needs `FoodFound` + `BasementKey` and no page (its basement task reads *Look for food*). Days 2–4 now also wake at **Floor 4 / Bed** (the bed the player sleeps in after each night) instead of Floor 2 / Bed. The tables below show the state before those changes.

## The new order

| Day | Wake up at | What happens | The day's puzzle is done when |
|---|---|---|---|
| 1 | Outside / Dock | Explore the house: each of Floors 1–4 plays a short line the first time you enter it | All four floors are visited (flags `ExploredFloor1`–`ExploredFloor4`) |
| 2 | Floor 2 / Bed | Radio call: "You need food, there might be some in the storage." Then find the key (Floor 4), open the basement, take the food and the journal | Journal page 1 is collected **and** `FoodFound`, `BasementKey` and `Journal` are set |
| 3 | Floor 2 / Bed, **power out** | Fix the power: electrical box in the basement, then the wire puzzle | Page 2 is collected **and** `PowerFixed` is set |
| 4 | Floor 2 / Bed | Horn outside, then the message in a bottle, then the book of symbols | Page 3 (the safe isn't built yet, so Day 4 can't finish) |

When the day's puzzle is done, the player is told to head to the top of the lighthouse. Arriving there starts the night.

---

## How a quest is tied to a day

A quest is tied to a day in only **two places**.

### 1. The Day asset (`Assets/Data/Days/Day1` … `Day4`)
Each Day asset decides what happens when you wake up and what ends that day's puzzle:

| Field | What it does |
|---|---|
| **Starts With Power Outage** | The power is off when the day starts (the 2D lights dim) |
| **Wake Dialogue** | The lines that play when you wake up |
| **Puzzle Task Id / Puzzle Task Text** | The task you get right after the wake dialogue. The id is what scene objects check; the text is what the task box shows |
| **Journal Page** | The page that must be collected (0 = none) |
| **Required Flags** | Flags that must also be set before the day's puzzle is done. They're also what **Start On Day** gives you for a skipped day (see *Testing a later day*), so list every flag that later days depend on |
| Start Scene / Start Spawn Id, Ship Count, Bedtime Cutscene | These belong to the **day number**, not the quest, so they were left alone |

### 2. Scene objects with "When Available" conditions
Interactables (Food, Journal, Key, Bottle…) and StoryTriggers each have conditions in the Inspector:
- **Active On Days** pins the object to a day **number**. **This is the only box you must change when a quest moves.**
- **Active During Tasks** and **Required Flags / Hidden By Flags** follow the quest automatically: if the quest moves to another day, the task moves with it, so nothing needs changing.

### What the code does with this (nothing to edit)
- `Assets/Scripts & Controllers/Day Flow/DayManager.cs`:
  - `BeginDay` (line 131) turns the power off when **Starts With Power Outage** is ticked (line 138).
  - `PlayWake` (line 218) plays the **Wake Dialogue**.
  - `StartPuzzle` (line 223) sets the **Puzzle Task**.
  - `CheckPuzzleDone` (line 231) waits for the **Journal Page** and every **Required Flag**.
- `Assets/Scripts & Controllers/Core/StoryConditions.cs`, `AreMet` (line 5): checks every object's Active On Days / Active During Tasks / Required Flags / Hidden By Flags boxes.

---

## The steps, in order

### Step 1: Move the Day asset settings
In the Project window, select each asset under `Assets/Data/Days/` and edit it in the Inspector.

**Work from the last day backwards** (Day 4 first, then 3, then 2, then 1). That way you copy a day's content into the next day *before* you overwrite it.

To copy a whole Wake Dialogue: right-click the **Wake Dialogue** label → **Copy**, select the other Day asset, right-click **Wake Dialogue** → **Paste**.

| Field | Day 1 | Day 2 | Day 3 | Day 4 |
|---|---|---|---|---|
| Starts With Power Outage | off | **off** (was on) | **on** (was off) | off |
| Wake Dialogue | unchanged (boat leaves) | **new: radio call** (below) | **old Day 2 lines** (power cut) | **old Day 3 lines** (horn) |
| Puzzle Task Id | `day1_explore` | **`day1_find_key`** (was `day2_power`) | **`day2_power`** (was `day3_check_horn`) | **`day3_check_horn`** (was `day4_todo`) |
| Puzzle Task Text | **`*Explore the house*`** | **`*Check around the house to find the key for the basement*`** | **`*Fix the power*`** | **`*Check the horn sound coming from outside*`** |
| Journal Page | **0** (was 1) | **1** (was 2) | **2** (was 3) | **3** (was 4) |
| Required Flags | **`ExploredFloor1`, `ExploredFloor2`, `ExploredFloor3`, `ExploredFloor4`** (was `FoodFound`) | **`FoodFound`, `BasementKey`, `Journal`** (was `PowerFixed`) | **`PowerFixed`** (was empty) | **empty** |

`BasementKey` and `Journal` were added to Day 2 later, for **Start On Day**. They change nothing in normal play: you always have the key and the journal by the time you find the food. But they mean that starting on Day 3 hands you the key, so you can get into the basement to the electrical box.

Day 2's new Wake Dialogue (3 lines; an empty Character Name shows no speaker):

| Character Name | Text |
|---|---|
| *(empty)* | `*Radio static*` |
| `Manager` | `You need food, there might be some in the storage.` |
| `Player` | `[PLACEHOLDER] The storage is down in the basement... and that door was locked. There has to be a key somewhere in the house.` |

### Step 2: Floor 1, the basement door
Open the `Floor 1` scene and select **Transitions/Basement Door**. In **Scene Transition**, under **Lock**, go to **When Tried While Locked** and empty both **New Task Id** (was `day1_find_key`) and **New Task Text**.

*Why:* trying the locked door used to start the key quest. Day 2's radio call now starts it, so on Day 1 the door just says "It's locked." **Locked Dialogue** and **Unlock Flag** (`BasementKey`) stay as they were.

### Step 3: Floor 4, the key
Open `Floor 4` and select **Basement Key**. In **Flag Pickup**, under **When Available**, change **Active On Days** from `1` to **`2`**. Its **Active During Tasks** (`day1_find_key`) stays, so the key only appears once Day 2's task is running.

### Step 4: Basement, the food, journal and arrival line
Open `Basement`:
- **Interactables/Food**: **Active On Days** `1` → **`2`**
- **Interactables/Journal**: **Active On Days** `1` → **`2`**
- **Story/Day 1 - Basement Arrival**: **Active On Days** `1` → **`2`**. I also renamed the object to **Day 2 - Basement Arrival** (the name is only a label).
- **Electrical Box**: nothing to change. It is tied to the task `day2_power`, which is now Day 3's puzzle task, so it moved to Day 3 on its own.

### Step 5: Outside, the bottle
Open `Outside`:
- **Interactables/Message in a Bottle**: **Active On Days** `3` → **`4`**
- **Story/Day 3 - Horn Check**: only renamed to **Day 4 - Horn Check**. It is tied to the task `day3_check_horn`, which is now Day 4's puzzle task.
- The Floor 3 books need nothing: they depend on the `BottleMessage` flag, not a day.

### Step 6: New Day 1 "explore" lines (Floors 1–4)
In each of the `Floor 1`, `Floor 2`, `Floor 3` and `Floor 4` scenes:
1. Hierarchy → right-click empty space → **Create Empty**, name it **Story**. None of the floors had a Story folder yet.
2. Right-click **Story** → **Create Empty**, name it **Day 1 - Explore Floor N** (N = the floor).
3. **Add Component** → **Story Trigger**, and set:
   - **Mode**: When In Scene
   - **Active During Tasks**: 1 element, `day1_explore`
   - **Remember As Flag**: `ExploredFloorN` (e.g. `ExploredFloor3`). This makes it play only once, and that flag is what Day 1's Required Flags wait for.
   - **Dialogue → Dialogue Lines**: 1 element, Character Name `Player`, and the text:
     - Floor 1: `[PLACEHOLDER] The ground floor. That door must lead down to the basement.`
     - Floor 2: `[PLACEHOLDER] The second floor. Cozy enough, I guess.`
     - Floor 3: `[PLACEHOLDER] Shelves full of old books up here.`
     - Floor 4: `[PLACEHOLDER] The fourth floor. Only the lamp room is left above me.`
   - Leave **Outcome** and **Then** empty.
4. Save the scene (**Ctrl+S**).

When the fourth flag is set, Day 1's puzzle is done. The usual "I should head up to the top of the lighthouse" line plays, and you can start the night.

### Step 7: Test
Press Play in `Outside` to start a new game, and play through it in order. To test one day quickly, use **Start On Day** (next section). Tested:
- **Day 1:**
  - The task is "Explore the house".
  - The basement door only says it's locked, and the task stays the same.
  - The key is hidden.
  - Each floor's line plays once.
  - After Floor 4 you get "head to the top".
  - The night has 3 ships.
- **Day 2:**
  - The power is on, and the radio call plays.
  - The task is the key.
  - The key appears, and picking it up moves the quest on.
  - The basement arrival line plays.
  - The food and journal are there; the electrical box isn't.
  - Page 1 + food finish the day.
- **Day 3:**
  - The power is out (lights dimmed) and the power-cut lines play.
  - The task is "Fix the power".
  - The electrical box is there; the food and journal aren't.
- **Day 4:**
  - The horn lines play.
  - Going outside plays the "What's that floating in the water?" lines.
  - The bottle is there.

---

## Testing a later day (Start On Day)

### Using it
1. In the Project window, select `Assets/Prefabs/Resources/Systems`. You don't need to open the prefab: selecting it shows its components in the Inspector.
2. In **Day Manager → Starting the Game**, set **Start On Day** to the day you want, e.g. `3`.
3. Open the `Outside` scene and press Play. The intro card is skipped. Days 1–2 count as done, and Day 3 starts at its own start spot (Floor 4 / Bed) with its wake dialogue.
4. A yellow warning in the Console reminds you it's on. **Set it back to `1`** when you're done.

"Counts as done" means you get each skipped day's **Journal Page** and **Required Flags**, and nothing else: no ship results, and no flags that aren't in a Required Flags list. That's why each day's Required Flags should include whatever later days need from it, like the basement key.

It only works when you press Play in the Editor. A build always starts on Day 1, even if you forget to reset it.

### The code (added to `Assets/Scripts & Controllers/Day Flow/DayManager.cs`)
- **Line 24:** the new field. It sits in the `Starting the Game` header group, so it shows in the Inspector:
  ```csharp
  [SerializeField] private int startOnDay = 1;
  ```
- **`Start()`, around line 77:** a new game in the first scene now checks it first:
  ```csharp
  if (Application.isEditor && startOnDay > 1) StartOnDay(startOnDay);
  else StartNewGame();
  ```
- **`StartOnDay(int day)` (line 103):** logs the warning, resets the game state, calls `GiveResults` for every earlier day, then `BeginDay(day)`. It's public, so other test tools can call it too.
- **`GiveResults(DayDefinition)` (line 116):** collects that day's Journal Page and sets its Required Flags.

---

## Checklist for the next time you move a quest
1. **Day assets:** move Wake Dialogue, Puzzle Task Id/Text, Journal Page, Required Flags and Starts With Power Outage. Work from the last day backwards.
2. **Active On Days:** in every scene the quest uses, check each of its objects for **Active On Days** and change the day number. To find them quickly, type `t:StoryTrigger`, `t:FlagPickup`, `t:FoundPage`, `t:InteractDialogue`, `t:ElectricalBox` or `t:SceneTransition` into the Hierarchy search box.
3. **Quest starters:** look for anything that starts the quest, i.e. a **New Task Id** in a When Tried While Locked, After Pickup, After Found or Outcome box. Make sure it can't start the quest on the wrong day.
4. **The day that loses the quest** needs a new way to end: something must set its Required Flags, or set Journal Page to 0 and leave Required Flags empty.
5. **Things later days need:** if a later day depends on something this quest gives you (a key, an unlocked door), add that flag to this day's **Required Flags**, so Start On Day hands it over.
6. **Night ships** (Night Scene → each `NightShip` → **Active On Nights**) only matter if the night should change too.
7. **Save every scene** you touched, then test from `Outside`, using **Start On Day** to jump to the day you changed.

---

## What was left as it was

- **Task ids keep their old day prefixes** (`day1_find_key` is now used on Day 2, `day2_power` on Day 3, `day3_*` on Day 4). Ids are only labels: they work on any day. To rename one, change it in **every** place listed below, or that step breaks.
- The flag **`Day1BasementArrived`** (the arrival line's Remember As Flag) keeps its name, for the same reason.
- **Ship Count** (3 / 5 / 7 / 9) and the **Bedtime Cutscene** cards stay with their day numbers.
- **Test Scene** wasn't touched.

### Where each task id is used

| Task id | Used in |
|---|---|
| `day1_explore` | Day1 → Puzzle Task Id; Floors 1–4 → Story/Day 1 - Explore Floor N → Active During Tasks |
| `day1_find_key` | Day2 → Puzzle Task Id; Floor 4 → Basement Key → Active During Tasks |
| `day1_open_basement` | Floor 4 → Basement Key → After Pickup → New Task Id |
| `day1_food_journal` | Basement → Story/Day 2 - Basement Arrival → Outcome → New Task Id |
| `day2_power` | Day3 → Puzzle Task Id; Basement → Electrical Box → Active During Tasks |
| `day3_check_horn` | Day4 → Puzzle Task Id; Outside → Story/Day 4 - Horn Check → Active During Tasks |
| `day3_observe_water` | Outside → Story/Day 4 - Horn Check → Outcome → New Task Id |
| `day3_find_book` | Outside → Message in a Bottle → New Task Id |
| `day3_open_safe` | Floor 3 → Book - Strange Symbols → New Task Id (replaced the old `day3_use_message`) |

Flags used by the story: `ExploredFloor1`–`4`, `BasementKey`, `Day1BasementArrived`, `FoodFound`, `Journal`, `PowerFixed`, `BottleMessage`, `BottleDeciphered`, `SafeOpened`.

The rest of Day 4 (the note, the safe, the last night) and the night timer are described in `Docs/Day 4 quest, night timer and ending.md`.

**If Day 1 ever stops working after a merge:** check the `Day1` asset first. Its Puzzle Task Id must be `day1_explore`, and its Required Flags must be `ExploredFloor1`–`4`. A merge once emptied these, and then the floor lines never play and Day 1 ends straight after the boat leaves.
