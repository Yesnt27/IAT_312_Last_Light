# Endings, night report, day cards and the animated player

## Counting ships (GameState)

Every ship the player deals with is counted twice: for **tonight** and for the **whole game**. Both are on the Systems prefab → **Game State → Night Results** (watch them change in Play mode on `Systems(Clone)`).

| Result | Counts as |
|---|---|
| Normal ship marked **Safe** (guided) | correct |
| Abnormal ship **Reported** (caught) | correct |
| Abnormal ship marked **Safe** (let through) | **failed** |
| Normal ship **Reported** (wrongly reported) | **failed** |
| Any ship **not reported** before the time ran out | **failed** |

**How it's counted in code:**
- **Where the numbers live:** `Core/ShipStats.cs` holds the five numbers plus `Correct`, `Failed` and `Ships`.
- **Recording:** `NightShip` / `NightShift` call `GameState.Record…()` as before, and each call now adds to both `GameState.Tonight` and `GameState.Total`.
- **Each night:** `DayManager.StartNight` clears Tonight.
- **New game:** both are cleared.

There are **24 ships** over the four nights (3 + 5 + 7 + 9).

## The bad ending

On the Systems prefab → **Day Manager → Ending**:
- **Max Failed Ships = 12.** If the player failed **more** than 12 ships over all four nights (13 or more), the **Death Cutscene** plays instead of Victory. Exactly 12 still wins.
  - **To change the rule:** change this number. 11 would mean "12 or more loses".
- **Death Cutscene → Scene = `Ending2`.**
- **Victory Cutscene → Scene = `Ending 1 - Win`** (unchanged).

`DayManager.IsBadEnding` is the check: `Total.Failed > Max Failed Ships`.

**The scene `Ending2`** (in Build Settings, right after Ending 1):
- **Canvas → Image:** the `defeat` painting. It's stretched to the whole screen with an **Aspect Ratio Fitter** set to *Envelope Parent* (ratio 1.824, from the 1970×1080 picture). It always covers the screen at any window shape and crops a sliver at the edges instead of showing bars. Its Canvas Scaler is *Scale With Screen Size*, 1920×1080.
- **Its own EventSystem** is switched off, like the other cutscene scenes (the Systems prefab has one).
- **Story → Ending Dialogue:** a Story Trigger copied from Ending 1 (*When In Scene*, Ends Cutscene off, so the game stays on the painting). Its lines are placeholders, so rewrite them freely in the trigger's **Dialogue**:
  1. *\*The fog swallows the lighthouse...\**
  2. *\*Too many ships slipped past your light. Tonight, one of them came back for you.\**
  3. Manager: *"Keeper? Do you copy? ...Keeper?"*

  Tick **Ends Cutscene** if it should return to the Title Screen after the lines.

## The night report

After each night, the report card shows **tonight's** results, then the running total of failed ships:

```
NIGHT 2 REPORT
Ships guided safely: 2
Abnormal ships caught: 3
Abnormal ships let through: 1
Normal ships wrongly reported: 1
Not reported in time: 2

Failed tonight: 4
Total failed ships: 7 of 14
```

"of 14" is the number of ships so far (3 on night 1 + 5 on night 2...), so it reads "7 of 24" after the last night.

**To change the text:** Systems prefab → **Day Manager → Night Report → Night Report Text**. The numbers go in with:
- `{0}` guided
- `{1}` caught
- `{2}` let through
- `{3}` wrongly reported
- `{4}` not reported
- `{5}` failed tonight
- `{6}` failed in all
- `{7}` ships in all so far

The code is `DayManager.NightReport()`.

## Day cards

Before each day starts, a black screen shows the day: **Day 1** (after the intro), **Day 2**, **Day 3**, **Day 4**.
- **Days 2–4:** it comes after the bedtime video or card.
- **Timing:** it fades the text in and out (about 3.6 s), then the day's scene fades in.
- **The player** is frozen while it shows.

**Changing it:**
- **The prefab:** `Assets/Prefabs/UI/Day Card` (`UI/DayCard.cs`). It has its own canvas, sorting 200, so it sits above everything.
  - **Day Text:** `Day {0}`, where {0} is the day number.
  - **Fade Seconds:** 0.6.
  - **Hold Seconds:** 1.8.
  - **Label:** change its font, size and colour.
- **Day Manager → Day Card:**
  - **Day Card Prefab:** empty = no cards.
  - **Show Day Card On First Day:** untick to skip "Day 1".

## The animated player

**What changed:**
- **Location:** your animated prefab is now **the** player at `Assets/Prefabs/Player.prefab` (moved from `Visual Assets/player/`).
- **The old one:** the old Player prefab without animations was deleted.
- **SceneController's Player Prefab** points at the new one.

**How it's built:**
- **Player** (root, tag `Player`):
  - **Rigidbody2D, Capsule Collider 2D, PlayerController, PlayerInteractor:** settings copied from the old player (speed 6, jump 13, same collider, same ground check). Doors, stairs and spawns behave exactly as before.
  - **PlayerController → Art Faces Right:** on, because the new drawing faces right.
- **Art:** **Animator** (`Visual Assets/player/player.controller`) + **Sorting Group** (order 10, so the whole character draws in front of the room like the old sprite).
  - **Size:** Art's **Scale** sets the character's size. It's 0.17, so the character is about as tall as the old one. Your test copy in Outside used 0.2, which is about 20% bigger; if you change it, check that the head still clears doorways.
  - **body_0:** your rig (head, arms, legs), animated by the idle / walk / jump clips.
- **Ground Check:** same place as before.
- **PlayerAnimation** (on the root): drives the Animator from PlayerController:
  - **IsWalking** = moving left or right.
  - **IsJumping** = in the air (straight away when jumping up; after 0.15 s when falling, so walking down stairs stays a walk).
  - **When frozen:** the player stays idle during dialogue and puzzles. The old version read the keys directly.

**Removed scene copies:** two hand-placed players were deleted. They would have become extra players:
- **Outside:** your `player` test copy.
- **Top Lighthouse:** an old `Player` copy.

The game spawns the player itself at each scene's spawn point. `Prefabs/_Recovery/0.unity`, a crash-recovery backup scene, still points at the old prefab. It isn't used by the game, so delete that folder whenever you like.
