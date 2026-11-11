# Zoom music, title text and font

How the Outside zoom-out plays music and shows a title, and where the game's font is set.

The zoom zone is `Outside` → **zoom_collider** (Camera Trigger). Its **Events** section has two lists:
- **On Zoom Out** runs when the player walks into the zone.
- **On Zoom In** runs when they walk out.

Anything you add to these lists happens at those moments, with no code.

## Only the first time

The camera zooms out every time the player walks into the zone. The music and the **Last Light** title only come with the **first** zoom-out of a game; after that, the camera zooms on its own.

This is done by a **Play Once Per Game** component (`Core/PlayOncePerGame.cs`) on `zoom_collider`:
- Camera Trigger → **On Zoom Out** has one entry: *zoom_collider → PlayOncePerGame.Play*.
- Play Once Per Game → **On First Time** holds what used to be in On Zoom Out: *Zoom Title → FadeGroup.Show* and *zoom_music → MusicFader.FadeIn*.
- **Remember As Flag** (`ZoomIntroPlayed`) is the GameState flag that records it has played. A new game clears all flags, so a new playthrough gets the intro once more.
- **On Zoom In** still fades both out. That's harmless once they've stopped, and it still cuts the music short if the player leaves the zone during the first zoom.

**To make something happen every time again**, move it from On First Time back into On Zoom Out. Play Once Per Game works for any other one-time beat too: give it its own flag name.

---

## Music when it zooms out (set up)

`Outside` → **zoom_music** plays `Assets/Visual Assets/Audio Assets/puppet_combo.mp3.wav`, an 8.6-second track, once:
- It starts at full volume the first time the camera zooms out (see **Only the first time**).
- It fades out over its last 2 seconds instead of cutting off.
- Walking out of the zone early fades it out over 2 seconds.

**zoom_music has two components:**
- **Audio Source:**
  - **AudioClip** = `puppet_combo`
  - **Play On Awake** = off (otherwise it plays as soon as the scene loads)
  - **Loop** = off, so it plays once and fades at its end. With Loop on, it repeats until the player leaves the zone.
  - **Volume** = 0.609: how loud the music gets. Change it here.
  - **Spatial Blend** = 0 (2D)
- **Music Fader** (`Audio/MusicFader.cs`):
  - **Fade In Seconds**: 0 starts at full volume; e.g. 1 fades in.
  - **Fade Out Seconds** (2): used both for the fade at the end of the track and when the player leaves the zone.
  - **Fade Out At End** (on): with Loop off, fade over the last Fade Out Seconds of the track.
  - **Resume Where It Stopped** (off): off restarts the track every zoom; on continues from where it faded out.

**The hook-up** is on `zoom_collider`:
- **Play Once Per Game → On First Time** → *zoom_music → MusicFader.FadeIn* (Camera Trigger's On Zoom Out calls Play Once Per Game).
- **Camera Trigger → On Zoom In** → *zoom_music → MusicFader.FadeOut*.

The fade commands only appear in an event's function list under **MusicFader**, and only when the object in the slot has a Music Fader component. An Audio Source on its own only offers Play / Stop / Pause, which have no fade.

**To use a different track:** set it as the Audio Source's AudioClip. Nothing else changes.

**Optional:** select the audio file and set **Load Type** = *Streaming* in the Inspector (better for music), then press **Apply**.

The scene's Main Camera already has the **Audio Listener** Unity needs to hear sound. Don't add a second one.

---

## The title text when it zooms out (already added)

`Outside` → **Zoom Title** shows **Last Light** at the top of the screen. It fades in as the camera zooms out and fades out as it zooms back in.

| Object / component | What to change there |
|---|---|
| **Zoom Title** → Canvas | **Sort Order** 50: above the game, under the dialogue box and screen fades (the Systems UI is 100) |
| **Zoom Title** → Canvas Group | Leave **Alpha** at 0: the fader controls it in Play. **Blocks Raycasts** is off, so it never steals clicks |
| **Zoom Title** → Fade Group | **Fade Seconds** (1.5) = fade speed. **Start Hidden** = invisible until the first zoom |
| **Zoom Title/Title** → TextMeshPro - Text (UI) | The words (*Text Input*), **Font Size** (120), colour, alignment |
| **Zoom Title/Title** → Rect Transform | **Pos Y** (−150) moves it up or down from the top of the screen |

The hook-up is on `zoom_collider`: **Play Once Per Game → On First Time**: *Zoom Title → FadeGroup.Show*, and **Camera Trigger → On Zoom In**: *FadeGroup.Hide*.

- **More lines** (e.g. a subtitle): duplicate **Title** inside Zoom Title and move it. Everything inside Zoom Title fades together.
- **To see it while editing:** set Canvas Group → Alpha to 1, then put it back to 0.
- It shows **once per game**: the first time the player walks into the zone. Later zoom-outs (coming out of the lighthouse, the Day 4 horn check) don't show it again.

`UI/FadeGroup.cs` works on any UI with a Canvas Group: call **Show** / **Hide** from any event.

---

## The font: Love Ya Like A Sister

- **The font file:** `Assets/Visual Assets/LoveYaLikeASister-Regular.ttf`, which you added.
- **The font asset text uses:** `Assets/Visual Assets/LoveYaLikeASister-Regular SDF.asset`, made with *Create → TextMeshPro → Font Asset → SDF*.
  - It holds the 199 characters the game's text uses: English letters, digits, punctuation, curly quotes, accented Latin letters, and … – —.
  - It's set to **Static**, so it doesn't rewrite itself every time you press Play (the old Liberation fallback font did).
  - Anything it lacks falls back to **LiberationSans SDF**. That includes the Greek/Cyrillic cipher symbols on the bottle's note and in the symbol book.

**Where it's used:**
- **The Systems prefab UI:** dialogue box (name, text, *Click to continue*), task box, interaction prompt, cutscene cards, inventory labels.
- **Paper Note, Wire Puzzle, Safe Keypad, Bookshelf Puzzle.**
- **Night Scene:** binocular HUD, night timer, and the `[Q] Report` / `[E] Safe` labels.
- **It's also the TextMeshPro default font**, so new text you create uses it automatically (*Edit → Project Settings → TextMesh Pro → Settings → Default Font Asset*).

**Not changed:**
- the in-world placeholder labels (`EXIT`, `BOOKSHELF`, room names, `PAGE`);
- the Title Screen and How To Play menus.

To change one: select the text, and in its TextMeshPro component set **Font Asset** to `LoveYaLikeASister-Regular SDF`.

**If a character shows up in the plain font** (it's missing from the asset):
1. Select the font asset.
2. Set **Atlas Population Mode** to *Dynamic*.
3. Press Play once so the character gets added.
4. Set it back to *Static*.
