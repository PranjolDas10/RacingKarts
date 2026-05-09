# RacingKarts

A 3D kart racing game built in Unity (C#), featuring AI opponents, real-time standings, a nitro boost system, and a WebGL build deployed to AWS S3.

---

## Highlights

- **Ackermann steering geometry** — inner and outer front wheels follow independent arcs through corners, eliminating tyre scrub and producing realistic handling
- **Three drivetrain modes** — front-wheel, rear-wheel, and all-wheel drive with correct torque split per configuration
- **Event-driven architecture** — a static `RaceEventBus` decouples publishers (vehicle physics) from subscribers (HUD, audio, camera) using C# `Action` events, so each system is independently testable
- **Modular AI opponents** — waypoint-following AI with three difficulty presets (Easy / Medium / Hard) that scale acceleration and look-ahead distance; throttle eases automatically in tight corners
- **Data-driven kart config** — `KartStats` ScriptableObject lets designers hot-swap kart presets without touching code
- **Generic object pool** — eliminates per-frame `Instantiate`/`Destroy` overhead for particle effects
- **Audio manager** — singleton `AudioManager` wires engine pitch to live speed data and reacts to race events without polling

---

## Architecture

```
┌─────────────────────────────────────────────────────────┐
│                      RaceEventBus                        │
│  OnRaceStarted │ OnSpeedChanged │ OnNitroChanged │ ...   │
└────────┬────────────────┬──────────────────┬────────────┘
         │ publishes      │ publishes        │ publishes
         │                │                  │
  ┌──────▼──────┐  ┌──────▼──────┐  ┌───────▼───────┐
  │PlayerControl│  │  Gamemanager│  │ AudioManager  │
  │ler (physics)│  │ (standings  │  │ (engine pitch,│
  │             │  │ + countdown)│  │  race music)  │
  └──────┬──────┘  └─────────────┘  └───────────────┘
         │ reads                   ┌─────────────────┐
  ┌──────▼──────┐                  │ CameraController│
  │InputManager │                  │ (FOV on boost)  │
  │ (keyboard / │                  └─────────────────┘
  │  AI path)   │
  └─────────────┘
```

---

## Project Structure

```
Assets/
├── Pranjol's Assets/
│   └── Scripts/
│       ├── Core/
│       │   └── RaceEventBus.cs        # Observer-pattern event bus
│       ├── Audio/
│       │   └── AudioManager.cs        # Singleton; reacts to race events
│       ├── Data/
│       │   └── KartStats.cs           # ScriptableObject kart preset
│       ├── Utilities/
│       │   └── ObjectPool.cs          # Generic pool (eliminates GC spikes)
│       ├── PlayerController.cs        # Physics: drivetrain, braking, nitro
│       ├── InputManager.cs            # Keyboard + AI waypoint controller
│       ├── Gamemanager.cs             # Race state, countdown, standings
│       ├── CameraController.cs        # Smooth follow + boost FOV
│       ├── CarEffects.cs              # Nitro particle emitter
│       ├── TrackWayPoints.cs          # Waypoint path + editor gizmos
│       └── vehicle.cs                 # Race data record per vehicle
└── DownloadedAssets/                  # Third-party art assets
```

---

## Controls

| Action        | Key                |
|---------------|--------------------|
| Accelerate    | W / Up Arrow       |
| Brake/Reverse | S / Down Arrow     |
| Steer         | A / D / Arrow Keys |
| Nitro Boost   | Left Shift         |
| Handbrake     | Space              |
| Restart Race  | UI Button          |

---

## Key Systems

### RaceEventBus (`Core/RaceEventBus.cs`)
Static publish/subscribe hub. Publishers fire events (e.g. `PublishSpeedChanged`) with no knowledge of who listens. Subscribers (HUD, audio, camera) register via `+=` in `OnEnable` and deregister in `OnDisable`, preventing memory leaks across scene reloads.

### PlayerController (`PlayerController.cs`)
Drives `WheelCollider`-based physics. Per physics tick it:
1. Applies **Ackermann steering** — inner angle `atan(L / (R − t/2))`, outer `atan(L / (R + t/2))`
2. Distributes motor torque to driven wheels based on drivetrain type
3. Manages nitro charge (drain while boosting, recharge otherwise)
4. Publishes speed and nitro state to `RaceEventBus`

A `KartStats` ScriptableObject can override all tuning values at `Start`, enabling per-kart presets without code changes.

### InputManager (`InputManager.cs`)
Dual-mode component on every kart:
- **Player mode** — reads Unity input axes each `FixedUpdate`
- **AI mode** — scans all waypoints to find the closest node, targets `distanceOffset` nodes ahead, and modulates throttle based on required steering magnitude

Difficulty presets adjust acceleration and look-ahead distance, producing distinct opponent behaviors from a single code path.

### Gamemanager (`Gamemanager.cs`)
Orchestrates the race lifecycle: countdown freeze/unfreeze, standings sort, and finish detection. Uses `List<T>.Sort` with a comparison delegate (O(n log n)) for standings. Subscribes to `RaceEventBus` events to push reactive updates to the speedometer needle, nitro slider, and position display.

### AudioManager (`Audio/AudioManager.cs`)
Singleton that survives scene reloads via `DontDestroyOnLoad`. Subscribes to bus events to swap music, play one-shot SFX, and pitch-shift the engine audio loop proportional to current speed — without any vehicle script holding an audio reference.

### ObjectPool (`Utilities/ObjectPool.cs`)
Generic pool parameterised on any `Component` subtype. Pre-warms a set of inactive instances at construction time, eliminating allocation spikes during gameplay.

---

## Building & Running

1. Open `CulminatingPranjolFINAL/` in Unity 2021.3+
2. Open `Assets/Pranjol's Assets/Scenes/Pranjol Culminating.unity`
3. **Play in Editor** — press Play in Unity
4. **WebGL Build** — File → Build Settings → WebGL → Build

### AWS S3 Deployment
```bash
aws s3 sync ./Build s3://<your-bucket>/ --acl public-read
aws s3 website s3://<your-bucket>/ --index-document index.html
```

---

## Dependencies

| Package | Version |
|---------|---------|
| TextMesh Pro | 3.0.6 |
| Unity UI | 1.0.0 |
| Unity Vehicles (WheelColliders) | 1.0.0 |

---

## Asset Credits

- **Track** — communityMap (Unity Asset Store)
- **Skyboxes** — AllSky Free by rpgwhitelock
- **Trees** — Yughues Free Palm Trees
- **Vehicles** — various free Asset Store packs (Chiron, DB11, Porsche)
