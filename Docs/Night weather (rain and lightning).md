# Night weather: rain and lightning

The Night Scene has a **Weather** object with rain and lightning that get worse night by night.

## The settings you'll change most

Open **Night Scene → Weather**. Its **Night Weather** component has a **Nights** list: **Element 0 = night 1**, Element 1 = night 2, and so on. Nights past the end of the list use the last entry.

| Field | What it does | Now (nights 1 / 2 / 3 / 4) |
|---|---|---|
| **Rain Per Second** | How many drops fall each second. 0 = a dry night. About 2200 is steady rain; 4000 and up is a downpour | 500 / 1200 / 2200 / 4000 |
| **Wind** | How hard the rain is pushed sideways, in metres per second. 0 = straight down | 1 / 2 / 3.5 / 5 |
| **Lightning Per Minute** | Strikes per minute, on average. 0 = no lightning | 1 / 2 / 3.5 / 6 |

**Trying a night:**
- Press Play in the Night Scene. It uses the night set in **Night Shift → Preview Night** (1 now). Set it to 4 to see the worst night, and set it back when you're done.
- In a real game the night comes from the day loop.
- **Changing the numbers while playing takes effect at once**, so you can tune by eye. Unity forgets Play-mode changes when you stop, so write down the numbers you liked and type them in again afterwards.
- To make a bolt strike right now in Play mode: right-click the **Lightning Storm** component's title → **Strike Now (Play mode)**.

## The rain

**Weather → Rain** is a Particle System set up after SpeedTutor's *EASY Rain Particle in Unity* tutorial:

| Tutorial step | Here |
|---|---|
| Velocity over Lifetime, Y between −25 and −35 | same; Z is the night's **Wind** (the tutorial's "more angled rain") |
| Renderer: Stretched Billboard | same, material `Visual Assets/Materials/Rain` |
| Start Size 3D: X 0.1, Y 1–2, Z 0.12; Start Speed 0 | same |
| Start Colour alpha about 50% | same (a pale blue) |
| Colour over Lifetime: fade in and out | same |
| Emission about 100 | the night's **Rain Per Second** (the area is much bigger than the tutorial's) |
| Collision: World, Dampen 1, Bounce 0 | same; **Lifetime Loss is 1**, not 0.5, so each drop makes exactly one splash and is gone |
| Sub Emitter on Collision: Splash | **Rain → Splash**: Horizontal Billboard, ring texture at 70% alpha, duration 2, lifetime 2, speed 0, size 0.5, simulation speed 2, one burst of 1, fade in/out, grows over its life |

**What's different from the tutorial, and why:**
- **Shape:** a ring around the lighthouse (Circle, radius 55, with a 12 m hole in the middle), not a box. A box over the tower would rain inside the lamp room.
- **Rain Shelter:** an invisible capsule around the lamp room. Any drop the wind blows into it is removed (the Particle System's **Triggers** module, Inside = Kill), so it never rains indoors however strong the wind is.
- **Rain Sea Level:** an invisible floor at the sea, so drops stop there and leave splash rings on the water. Drops also splash on the island.
- **Prewarm** is on, so the sky is already full of rain when the night starts.

**Changing the look** (on **Rain**):
- **Colour and how see-through:** Start Color.
- **Streak size:** Start Size (X = thickness, Y = length).
- **Fall speed:** Velocity over Lifetime → Y.
- **How far out it rains:** Shape → Radius.

The ring picture is `Visual Assets/Generated/Weather/Rain Splash Ring.png`.

## The lightning

**Weather → Lightning** follows Gabriel Aguiar's *Unity VFX - Lightning Strike Effect Tutorial*. The project has no VFX Graph, so it's built the other way the video mentions (a mesh plus scripts), with the same parts:

| Tutorial part | Here |
|---|---|
| Zigzag mesh, thick at the top, thin at the bottom (Blender) | `LightningStrike` builds a new zigzag ribbon for **every** strike, with side branches, so no two bolts look alike |
| Bolt texture + a blurred glow copy (Krita) | `Visual Assets/Generated/Weather/Lightning Strike 01.png` and `…01_Blur.png`. They were painted by a script; you can paint your own over them (white on transparent, tiling top to bottom) |
| Two outputs, blur drawn first | **Lightning Strike → Blur** and **Bright** (materials `Lightning Blur`, `Lightning Bright`) |
| Erode (dissolve) shader with Clip | `Visual Assets/Shaders/Lightning.shader` ("Lighthouse/Lightning") |
| Colour gradient: a blink, then the flare | **Bright Color** / **Blur Color** gradients, times **Bright Intensity** (6) / **Blur Intensity** (2.5) |
| Clip curve: eroded at the start and at the end | **Clip** curve |
| Texture scrolls down at the brightest moment | **Scroll** curve |
| Blur lives a little longer (1.2) | **Blur Lifetime** |
| A second burst so it blinks twice | **Lightning Storm → Second Flash Chance** (0.4) and **Second Flash Delay** |

One thing was added that the tutorial doesn't have: **Lightning Flash**, a light that lights the sea, the ships and the room while the bolt is bright. It only changes how things look; it doesn't count as the searchlight for selecting ships.

**Lightning Storm** (on **Lightning**) decides when and where:

| Field | Meaning |
|---|---|
| **Strikes Per Minute** | Overwritten by tonight's **Lightning Per Minute**, so change it on Night Weather instead |
| **Distance** | How far out bolts land (350–800 m; the ships are about 300 m out) |
| **Bearing** | The arc they land in, in compass degrees from the lighthouse (0 = north, 90 = east, −90 = west). −165 to 25 covers the west and north windows, where the ships are. −180 to 180 = all around |
| **Cloud Height / Sea Level** | Where bolts start and end |
| **Lean** | How much a bolt may tilt |

**Lightning Strike** (on **Lightning → Lightning Strike**) is the look of one bolt:
- **Bolt Shape:** **Segments** and **Zigzag** set how jagged it is. **Width** is the ribbon's width at the top and bottom; bigger = a fatter bolt and a wider glow. **Branches** is how many side forks.
- **Lifetime:** how long a strike lasts (1 second).
- **Flash Intensity:** how brightly the strike lights the scene (1.2). Set it to 0 for no flash.

If you break a curve or gradient, the component's **⋮ → Reset** brings back the tutorial's values (then drag Bright, Blur and Lightning Flash back into its three reference slots).

## Scripts

All in `Assets/Scripts & Controllers/Night/`:
- **`NightWeather.cs`:** the per-night list. Applies tonight's rain, wind and lightning.
- **`LightningStorm.cs`:** picks when and where to strike.
- **`LightningStrike.cs`:** builds and animates one bolt.
- **`NightShift.cs`:** gained `Night` (tonight's number), which NightWeather reads.
