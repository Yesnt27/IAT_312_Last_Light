# Video cutscenes (the bedtime clips)

The bedtime cutscenes are now videos. They're in `Assets/Visual Assets/videos/`:

| Clip | Plays | Length | Dialogue box |
|---|---|---|---|
| `sleep 2` | Day 1's bedtime (after the first night): going to sleep | 4.1 s | none |
| `sleep 1` | Day 2's bedtime: a shadow passes by | 3.4 s | none (the clip shows it) |
| `sleep 3` | Day 3's bedtime | 7.5 s | none |
| `sleep 2` | Day 4's bedtime (the last one, before the ending) | 4.1 s | Manager: *"We're coming to pick you up tomorrow."* for the whole clip |

Day 4's bedtime used to be a text card (the radio call); it's now the `sleep 2` clip with the Manager's line, and it leads into the ending.

**When they play:** the player presses E on the bed, the screen fades to black, and the clip plays full screen. The dialogue box shows on top at the set times. When the clip ends, the screen is black again and the next morning loads. **Esc skips** a clip. The player can't move while it plays.

## Changing the clip for a day

1. Open the day's asset: `Assets/Data/Days/Day1`, `Day2`, `Day3` (or `Day4`).
2. Under **Bedtime → Bedtime Cutscene**, drag a clip from `Visual Assets/videos` into **Video**.
   - **Video empty** = the old text card (Title + Description) shows instead.
   - **Scene** set = that cutscene scene wins over the video.
3. The intro and the ending cards on the **Systems** prefab's **DayManager** (Intro / Ending / Victory / Death Cutscene) have the same **Video** slot, so a clip can go there too.

New clips: put the `.mp4` in `Visual Assets/videos`. Unity imports it as a Video Clip by itself.

## Changing the dialogue and its timing

On the same **Bedtime Cutscene**, open **Video Lines**. Each entry is one line in the dialogue box:

| Field | What it does |
|---|---|
| **Line → Character → Name** | Speaker name above the text. Leave it empty for no speaker (for a sound cue like *\*Static\**). |
| **Line → Text** | What the box says. |
| **Start Time** | Seconds into the clip when the line appears. `0` = from the start. |
| **End Time** | Seconds into the clip when it goes away. **`0` = stays until the clip ends.** |

- **Day 4 now:** one line (Manager), Start 0 / End 0, so the box is on screen for the whole 4.1 s clip.
- **More lines:** press **+** and give them back-to-back times. For example, line 1 from 0 to 2 and line 2 from 2 to 0 (the end).
- **Gaps:** between two lines' times, the box hides.
- **Overlaps:** if two lines' times overlap, the one lower in the list shows.
- **Clicks:** the lines aren't clicked through. The **Click to continue** hint is hidden for them, and they go away on their own.
- **Clip lengths:** shown when you select a clip in the Project window (and in the table above). Times past the end of the clip never show.

## Where it lives

- **`UI/VideoCutscene.cs`:** on the Systems prefab → **UI Canvas → Video Cutscene** (a **Video Player** + the script).
  - Its **Panel** child is the black full-screen background. **Panel → Screen** is the picture, which an Aspect Ratio Fitter keeps at the clip's shape, with black bars if needed.
  - **Can Skip** turns Esc skipping on or off.
- **Layering:** Video Cutscene sits just above the inventory and just below the **Dialogue Box** in the canvas, so the dialogue box draws on top of the video. The black fade is lifted while a clip plays (the Panel is black itself) and put back after.
- **`UI/CutsceneCard.cs`:** each cutscene's **Video** and **Video Lines** (a list of `TimedLine`). The order it checks is Scene, then Video, then the card.
- **`DialogueManager.ShowTimedLine` / `HideTimedLine`:** show and hide a line without clicks. The normal dialogue (`StartDialogue`) is unchanged.
- **`DayManager.PlayCutscene`:** plays the video when a card has one.

## The console note about timestamps

Unity may log *"Unexpected timestamp values detected… not encoded with the baseline profile"* for these clips. It's harmless: Unity fixes the timing itself, and the clips play normally. To get rid of it, select the clip and tick **Transcode** in its import settings, or export the video with the H.264 **Baseline** profile.
