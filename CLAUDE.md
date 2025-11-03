# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project Overview

**SampleGodotCSharpProject** is a Godot 4.5 .NET game demonstrating component-based architecture, global event systems, and game mechanics patterns in C#.

- **Engine**: Godot 4.5
- **Language**: C# (.NET 8.0)
- **Main Scene**: `res://Game/Main.tscn`
- **Game Type**: Top-down 2D zombie survival game (prototype)

## Architecture: Component-Based Entity System

The project uses a **composition-over-inheritance** pattern with a custom component system:

### Core Structure

1. **Entity Layer** (`Game/Entity/`)
   - `Fireball`: Player-controlled flame projectile (follows mouse)
   - `Zombie`: Enemy unit with state machine (Idle/Walk states)
   - Both extend `CharacterBody2D` and compose multiple components

2. **Component Layer** (`Game/Component/`)
   - **BaseComponent**: Abstract base for all components with enable/disable support
   - **Core Components**:
     - `VelocityComponent`: Movement, gravity, collision handling
     - `HealthComponent`: Health management and visual representation
     - `FacingComponent`: Rotation based on movement direction
   - **Gameplay Components**:
     - `ExplosionComponent`: Death explosion and screen shake effect
     - `ScoreAttractorComponent`: Corpse attraction to player
     - `QueueFreeComponent`: Deferred node cleanup
   - **Follow Components** (interfaces for AI):
     - `FollowMouseComponent`: Player movement
     - `FollowPlayerComponent`: Enemy AI
     - `FollowPathComponent`: Patrol path following
   - **Element Components** (damage system):
     - `ElementComponent`: Base element system with energy
     - `FireComponent`: Fire spreading and damage

3. **Global Systems** (`Game/Autoload/`)
   - `GameEvents`: Central signal hub for all major game events
     - `ZombieKilled`, `ZombieSpawned`, `PlayerHit`, `Collision`, `ElementIntensity*`
     - Allows loose coupling between systems (Score, ZombieCounter subscribe here)
   - `Global`: Singleton for shared references (Camera2D, HUD, etc.)

4. **Managers** (`Game/Manager/`)
   - `FxManager`: Screen shake and visual effects
   - `SoundManager`: Audio playback and GameEvents subscription

5. **UI System** (`Game/UI/`)
   - All UI elements are Labels with event subscriptions
   - `Score`: Listens to `PlayerHit` events
   - `ZombieCounter`: Tracks `ZombieSpawned`/`ZombieKilled`
   - `RateStats`: Displays hit rate statistics
   - `Fps`: Real-time FPS display

### Key Design Patterns

- **Composition**: Entities contain multiple components rather than inheritance chains
- **Global Signals**: `GameEvents` broadcasts major events; systems subscribe rather than directly calling
- **Node Wiring**: `[Node]` and `[Export]` attributes with `WireNodes()` extension for auto-connection
- **State Machine**: `DelegateStateMachine` manages zombie state transitions (Idle ↔ Walk)
- **Extension Methods**: Custom utilities in `GodotUtilities/Extension/` add missing Godot helpers

## Command Reference

### Build & Compilation
```bash
# Restore dependencies and build C# project
dotnet build

# Build in Release configuration
dotnet build -c Release

# Clean build artifacts
dotnet clean
```

### Running the Project
```bash
# Open in Godot Editor (requires Godot 4.5+ installed)
godot --path . --editor

# Run game directly (without editor)
godot --path .
```

### Code Quality
```bash
# Format C# code with Rider code style (uses .editorconfig)
# In Visual Studio Code: Install C# extensions and use Format Document
# In JetBrains Rider: Code → Reformat Code (Ctrl+Alt+L)

# Note: Project uses OmniSharp for VSCode formatting consistency
```

## Project Layout

```
SampleGodotCSharpProject/
├── Game/                           # Game-specific code
│   ├── Main.cs                     # Scene manager, zombie spawning
│   ├── Autoload/
│   │   ├── Global.cs               # Singleton for shared state
│   │   └── GameEvents.cs           # Event hub with all game signals
│   ├── Entity/
│   │   ├── Fireball.cs             # Player (mouse-following flame)
│   │   └── Enemy/
│   │       ├── Zombie.cs           # Enemy unit with state machine
│   │       └── BaseEnemy.cs
│   ├── Component/                  # Composition-based components
│   │   ├── BaseComponent.cs
│   │   ├── VelocityComponent.cs    # Movement & collision
│   │   ├── HealthComponent.cs      # Health tracking
│   │   ├── FacingComponent.cs      # Direction rotation
│   │   ├── ExplosionComponent.cs   # Death effects
│   │   ├── Element/
│   │   │   ├── ElementComponent.cs # Base element system
│   │   │   └── FireComponent.cs    # Fire damage
│   │   └── Follow/
│   │       ├── FollowMouseComponent.cs
│   │       ├── FollowPlayerComponent.cs
│   │       ├── FollowPathComponent.cs
│   │       └── IFollowComponent.cs # Marker interface
│   ├── Manager/
│   │   ├── FxManager.cs            # Screen shake, effects
│   │   └── SoundManager.cs         # Audio management
│   ├── UI/
│   │   ├── Score.cs                # Point display with animation
│   │   ├── ZombieCounter.cs        # Enemy count tracker
│   │   ├── RateStats.cs            # Hit rate statistics
│   │   └── Fps.cs                  # FPS display
│   └── Helpers/
│       └── PointGenerator.cs       # Spawn point generation
│
├── GodotUtilities/                 # Reusable utilities from @firebelley
│   ├── Extension/                  # Extension methods for Godot classes
│   │   ├── NodeExtension.cs        # Resource instantiation, delayed adds
│   │   ├── Node2DExtension.cs      # Color intensification, rotation
│   │   ├── VectorExtension.cs      # Vector utilities
│   │   └── ... (10 extension files total)
│   ├── Logic/                      # State machine implementations
│   │   ├── StateMachine.cs         # Base state machine
│   │   ├── DelegateStateMachine.cs # Used by Zombie.cs
│   │   ├── ImmediateStateMachine.cs
│   │   └── LootTable.cs            # Weighted random selection
│   ├── Util/                       # Utility classes
│   │   ├── MathUtil.cs             # Math helpers
│   │   ├── Logger.cs               # Debug logging
│   │   ├── FileSystem.cs           # File operations
│   │   └── RaycastResult.cs        # Physics query helpers
│   ├── Collections/
│   │   └── DoubleDictionary.cs     # Bidirectional dictionary
│   ├── ParentNodeAttribute.cs      # [ParentNode] for auto-wiring
│   └── ChildNodeAttribute.cs       # [ChildNode] for auto-wiring
│
└── Game/Assets/                    # Art, audio, animations
    ├── Zombie/                     # Zombie sprites & animations
    ├── Explosions/                 # Explosion particle effects
    ├── Sounds/                     # Audio files
    └── Backgrounds/                # Background images
```

## Important Concepts

### Scene Structure (Main.tscn)

**Main.tscn** uses this hierarchy:
```
Main (Node2D) [Main.cs script]
├── WorldEnvironment (glow effects enabled)
├── Camera2D (drag-enabled for panning)
├── FxManager (screen shake effects)
├── SoundManager (audio management)
├── Entities (CanvasGroup with Y-sort for depth)
│   ├── FireBall (Fireball.cs - player)
│   └── ZombieFollowPath (test zombie following path)
└── Hud (CanvasLayer)
    ├── FPS (RealTime FPS display)
    └── Top Right (VBoxContainer with UI)
        ├── Timer (triggers timed events)
        ├── Score (Score.cs - displays points)
        ├── ZombieCounter (ZombieCounter.cs - displays enemy count)
        ├── HitRate (RateStats.cs - tracks hits)
        └── MaxRate (statistics)
```

### Event Flow Example: Zombie Death

1. `Zombie._CheckHealth()` detects health ≤ 0
2. Calls `ExplosionComponent.AddResourceAndQueueFree()`
3. Creates `ScoreAttractorComponent` (corpse attraction)
4. Emits `GameEvents.EmitZombieKilled()`
5. `Main.cs` subscribes and increments kill count, spawns new zombie
6. `ZombieCounter.cs` subscribes and updates display
7. `ScoreAttractorComponent` animates corpse to player
8. On collision, emits `GameEvents.EmitPlayerHit()`
9. `Score.cs` subscribes and increases points with animation

### Node Wiring System

The project uses custom attributes for automatic node connection:

```csharp
[Node]
public VelocityComponent VelocityComponent;

public override void _EnterTree()
{
    this.WireNodes();  // Extension method auto-finds child nodes
}
```

This replaces manual `GetNode()` calls, reducing boilerplate.

## Common Tasks

### Adding a New Component

1. Create `Game/Component/NewComponent.cs` extending `BaseComponent`
2. Implement `_Ready()` or `_PhysicsProcess()`
3. Use `[Node]` attributes for child node references
4. Call `this.WireNodes()` in `_EnterTree()`
5. Add to entity's scene in editor or instantiate via code

### Adding a New Game Event

1. Add Signal delegate to `GameEvents.cs` (e.g., `ElementIntensityMaxed`)
2. Add emission method (e.g., `EmitElementIntensityMaxed()`)
3. Subscribe in relevant systems via `GameEvents.Instance.SignalName += handler`

### Modifying Zombie Behavior

- **Movement**: Edit `VelocityComponent` properties or `FollowPlayerComponent`
- **Animation States**: Add states in `Zombie._Ready()` state machine setup
- **Damage**: Modify `HealthComponent` or `FireComponent` energy values
- **Spawn Behavior**: Adjust `Main.cs` spawn generation or `PointGenerator` parameters

### UI Updates

All UI is event-driven. Subscribe to `GameEvents` signals:
```csharp
GameEvents.Instance.ZombieKilled += _ => { /* update */ };
```

## Development Notes

- **Physics Ticks**: Set to 30/sec in `project.godot` for consistent physics
- **Screen Size**: 1152×640 (viewport), configured in `project.godot`
- **Y-Sort**: Entities use Y-axis sorting for pseudo-3D depth illusion
- **Glow Effects**: World environment has glow enabled for visual polish
- **Camera Drag**: Camera has drag margins for smooth panning (20% margins)

## Tools & Editor Setup

**Recommended Editor**: JetBrains Rider or Visual Studio Code + OmniSharp

**VSCode Extensions**:
- Godot Tools (geequlim)
- C# Tools for Godot (neikeq)
- EditorConfig Support

**Code Formatting**:
- Use `.editorconfig` for consistent style between editors
- Rider users: Code → Reformat Code
- VSCode users: Ensure OmniSharp formatting is enabled in settings

## Dependencies

- **Godot 4.5** (C# support)
- **.NET 8.0** (SDK)
- **Newtonsoft.Json 13.0.2** (JSON parsing for data)
- **GodotSharp 4.5.1** (Godot C# bindings)

All dependencies are managed via `SampleGodotCSharpProject.csproj`.
