# GAME DESIGN DOCUMENT & SYSTEM PROMPT
**Project Title:** Lofi Lunar: A Cozy Rover Journey (Working Title)
**Genre:** Cozy / Exploration / Driving Simulator / ASMR
**Engine:** Unity 3D (URP - Universal Render Pipeline)
**Art Style:** Stylized, Low-Poly, Dreamy, Flat-colored.
**Tone/Vibe:** WALL-E meets Journey. Melancholic yet warm, relaxing, zero-stress.

## 1. CORE VISION & ANTI-GOALS (CRITICAL RULES)
This is a therapeutic game about a lonely, adorable rover exploring a stylized moon, excavating human relics, and listening to Lofi space beats.
- ❌ **NO SURVIVAL OR FAIL STATES:** No health bars, no oxygen depletion, no hunger, no enemies, no combat, no death. The moon is safe and empty.
- ❌ **NO REALISM:** No gritty sci-fi textures, no normal maps, no high-poly realistic assets. Emphasize mood, lighting, and colors.
- ❌ **NO WHEEL COLLIDERS:** Never use Unity's default `WheelCollider`. It is too complex, jittery, and ruins the "chill" vibe.

## 2. THE CORE GAMEPLAY LOOP
1. **Drive & Chill:** Roam aimlessly across smooth, rolling lunar dunes in low gravity. Enjoy the ASMR sounds of tires on sand and floating moon dust.
2. **Ping & Excavate:** A gentle radar pings when near a buried "Human Relic" (e.g., retro Cassette Tapes, Gameboys, Rubik's cubes).
3. **Inspect & Catalog:** Unearthing an item opens a minimalist UI showing a beautiful 2D stylized icon of the item, along with a cute/naive text description written from the robot's perspective.
4. **Unlock Music:** Finding Cassette Tapes unlocks new Lofi tracks for the rover's built-in radio. The player can park, listen to music, and look at the giant Earth in the sky.

## 3. TECHNICAL ARCHITECTURE & MECHANICS

### A. The Rover Controller (Player)
- **Gravity:** Globally set to `-1.62` (Lunar Gravity).
- **Physics Approach:** Use the "Hidden Sphere Rigidbody" method. A hidden sphere handles physics (rolling/bouncing via `AddForce`). 
- **Visuals:** The visual 3D car mesh is a child object that smoothly tracks the sphere's position and uses Raycasts to align its rotation to the ground's surface normal (tilting gently on slopes).
- **VFX:** 
  - `TrailRenderer` on wheels for smooth, long-lasting tire tracks.
  - `ParticleSystem` emitting low-gravity, slow-floating dust puffs ONLY when the rover is grounded and moving.
  - Warm yellow `Spotlight` on the front to cut through the dark.

### B. Camera System (Cinemachine)
- **Type:** `Cinemachine FreeLook`.
- **Feel:** High damping on all axes (floaty, cinematic drone feel to absorb bumps). 
- **Auto-Recenter:** The player can freely orbit the mouse to look at the sky/Earth. If mouse input stops and the rover drives forward, the camera slowly and smoothly recenters behind the rover.

### C. Environment (The "Lunar Skatepark")
- **Terrain:** Procedurally generated (using `Mathf.PerlinNoise` via Editor Scripts) to create gentle, smooth rolling dunes. NO sharp spikes or steep mountains.
- **Material:** URP Lit, flat midnight-blue/dark-purple color (Hex: #1A1A2E). Slight specular highlight to make the moon dust "glitter" softly under the rover's spotlight.
- **Atmosphere:** Heavy use of URP Global Volume (Post-Processing). Bloom for glowing headlights and stars, Color Adjustments for a Lofi tint, and Exponential Fog to blend the horizon. A giant, vibrant Earth should be in the Skybox.

## 4. CODING & AI DEVELOPMENT GUIDELINES
- **Write clean, modular C# code:** Separate logic (e.g., `RoverMovement`, `RoverVFX`, `RelicScanner`).
- **Inspector Variables:** Use `[SerializeField]` extensively so the human designer can tweak variables (Speed, Forces, Damping, Colors) in the Inspector.
- **Event-Driven:** Use `UnityEvent` or `Action` for UI and Audio triggers to avoid tight coupling.
- **Editor Tools (MCP):** When asked to create environments or setups, write C# Editor scripts (`[MenuItem]`) to automate the process. Always include a cleanup step (e.g., `DestroyImmediate` existing generated objects) so the tool can be safely run multiple times.
## 5. HOME BASE & PROGRESSION LOOP (THE LOFI OASIS)
To provide long-term goals without stressful mechanics, the game features a central "Home Base" (an abandoned Lunar Lander) that the player restores and decorates.

**A. Dual Resource System:**
1. **Scrap (Currency):** Generic metal bits collected automatically by driving over them. Used for mechanical upgrades.
2. **Human Relics (Decor):** Unique nostalgia items found via Radar. Never destroyed. Used strictly for emotional decoration.

**B. Base Mechanics (Strictly NO Free-Form Grid Building):**
- **The Museum (Snap-Points):** To avoid physics bugs and UI complexity, the Base has predefined invisible "Snap-Points" on shelves/tables. Players press a 'Deposit' button, and found Relics automatically populate these spots, transforming the base into a cozy, cluttered nest.
- **The Radio Tower:** Players spend Scrap to upgrade the central antenna. Upgrading it expands the "Cozy Signal Radius" (visualized by glowing warm ambient lights and clearer Lofi music channels).
- **The Biodome:** A visual milestone. Players can plant rare seeds to grow a massive, bioluminescent alien tree over the base.
- **Lamp-posts (Trail of Lights):** Players can spend Scrap to drop small glowing lamp-posts while exploring the dark wilderness, eventually creating a player-made network of glowing trails back home.

**C. Rover Upgrades:**
Spend Scrap to unlock gameplay enhancers: Brighter/color-changing headlights, a wider Radar ping range, and a "Hover-Jump" suspension to leap over massive craters.
## 6. GATHERING & EXCAVATION MECHANICS (THE "CHILL" SYSTEM)
Strict Rule: NO voxel terrain deformation/digging. NO hitting rocks with pickaxes. NO stopping the flow of driving for basic resources. Harvesting relies purely on Magnetism, Light, and VFX.

**A. Scrap Gathering (Magnetic Breadcrumbs):**
- **Visuals:** Small, softly glowing gears/metal bits scattered in trails to guide player exploration.
- **Interaction:** Drive-by collection. When the rover is within a certain radius, scrap pieces smoothly float up, trail the rover for a split second, and `Vector3.Lerp` into the rover.
- **Audio (Crucial):** Emits a highly satisfying, ASMR "wind-chime" or "glass clink" sound upon collection. No button press needed.

**B. Relic Excavation (Musical Sonar & Tractor Beam):**
- **Phase 1 - The Hunt:** Player presses [Space] for a Sonar Ping. Hidden underground relics ping back with a glowing ground decal and a musical note (e.g., Kalimba). The closer the rover, the faster the pings (Hot/Cold radar).
- **Phase 2 - The Extraction:** Player parks over the glowing spot and holds [E]. The rover emits a glowing downward cone (Tractor Beam). 
- **Visual Trick:** Swirling dust particles hide the ground intersection. The hidden Relic smoothly floats up from beneath the terrain into mid-air, rotating gently.
- **Phase 3 - The Reveal:** The UI briefly shows a stylized 2D icon of the relic and a cozy/naive description before being collected.