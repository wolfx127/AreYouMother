# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project Overview

"AreYouMother" is a local-co-op extraction game (双人本地合作撤离游戏) built on Unity 6000.2.14. Two players (A = ranged, B = melee) enter a level together, defeat enemies, loot containers, and evacuate with their spoils. Between matches they share a warehouse, a dealer shop, and persistent player stats.

## Build & Run

- Open `AreYouMother.sln` in Rider/VS, or open the project folder in Unity Hub targeting Unity 6000.2.14.
- No CLI build command — build from the Unity Editor (File → Build Settings).
- No formal test suite exists; there is no `npm`, `dotnet test`, or CI pipeline. Unity Test Framework (`com.unity.test-framework`) is installed but no C# tests have been written.

## Key Dependencies

| Package | Role |
|---|---|
| `com.cysharp.unitask` (github) | Async/await for Unity — drives Buff ticks, attack cooldowns, delayed tasks |
| `com.unity.nuget.newtonsoft-json` | Polymorphic JSON serialization (`TypeNameHandling.All`) for save files |
| `com.unity.inputsystem` | Player input across both players (gamepad + keyboard) |
| `com.unity.render-pipelines.universal` | Rendering |

## High-Level Architecture

### Directory layout

```
Assets/
├── Scripts/
│   ├── Sword/          ← Combat framework (enemies, FSM, Buffs, projectiles)
│   │   ├── Frames/     →    Fsm.cs, TaskMgr.cs (generic infrastructure)
│   │   ├── GamePlay/   →    Enemy/, Buff/, Projectile.cs, StepUpMovement.cs
│   │   └── SO/         →    ScriptableObject configs (Enemy, Buff)
│   └── Taffy/          ← Game business logic (players, scenes, containers, home)
│       ├── OverAllManager/ → Scene mgmt, EventBus, EventList, global player state
│       ├── Home/        →    WarehouseManager, DealerManager, HomeHandler
│       ├── Play/        →    Player/, Container/, Place/, Enemy/, GameScenes/
│       ├── Data/        →    Prop, PlayerProfile, PlayerCurrentState, JsonData
│       └── UI/          →    UGUI UI managers (HomeUIManager, PlayingUIManager)
├── SBJC/               ← Player movement/camera/attack (third-party module)
├── Others/Taffy/       ← InputActions assets, prefabs, animations
└── StreamingAssets/    ← PropImages/, runtime assets
```

### Scene flow

```
Start → (async load Home additively, unload Start)
         Home ←→ Play (via EventBus scene-change events)
```

`OverAllSceneManager` (lives in Start scene, `DontDestroyOnLoad`) is the scene orchestrator. It uses additive scene loading — Home and Play scenes load/unload while the persistent Start scene holds cross-scene managers.

### Communication patterns

**Priority order** (as per project conventions):

1. **Singleton direct call** — preferred for same-system communication. Use `ClassName.Instance.method()`.
2. **Assigned script reference** — second choice when a reference is already wired in the Inspector.
3. **EventBus** (`Taffy.OverAllManager.EventBus`) — last resort for cross-system or decoupled communication. Each event is a **struct** defined in `EventList.cs`; the struct type IS the channel. Subscribe with `EventBus.Subscribe<T>(handler)`, publish with `EventBus.Publish(new T(...))`. Zero GC.

### Key singletons (in order of cross-referencing frequency)

| Singleton | Scope | Role |
|---|---|---|
| `PlayerCurrentStateController` | Play scene | HP/MP/Bag/ATK/DEF for both players during a match |
| `OverAllPlayerController` | Persistent (Start scene) | MaxHP/MaxMP/BagSize for both players across matches; owns pre-match bags |
| `BuffMgr` | Play scene | Manages Poison/Bleed Buffs per player, uses TaskMgr for tick timing |
| `BulletPool` | Play scene | Object pool for Bullet — shared by PlayerA (shoots EnemyB) and enemies (shoot players) |
| `EvacuateManager` | Play scene | Tracks evacuation/death of both players, publishes settlement events |
| `HomeHandler` | Home scene | Input handling for Bag/Warehouse/Dealer UI in Home |
| `PlayingHandler_A` / `PlayingHandler_B` | Play scene | Player movement, input routing, bag/container interaction |

### Prop (item) system

All items inherit from abstract `Prop` (namespace `Taffy.Data`). Behavior is added via interfaces:

- `IWeapon` (ATK) → Sword, Bow
- `IDefend` (DEF) → Armor
- `IRemoteAttack` (LaunchObject) → Bow
- `ICure` (Curative) → CurePotion
- `ICultivate` (BonusEffect, permanent stat buff) → HeartFruit, GiftBox
- `IUsable` (UseEffect, one-shot) → CurePotion, MagicMirror
- `ITreasure` (marker only, sellable) → Coin, TaffyPhoto, etc.

Each prop has an `owner` field (`PropOwner.A`, `B`, or `Public`) governing who can pick it up. Concrete classes use parameterless constructors to set default values; subclasses (e.g. `BigSword : Sword`) override only the fields that differ.

Props are serialized polymorphically via Newtonsoft `TypeNameHandling.All` — every JSON save includes `$type` so concrete types are preserved on deserialization.

**Prop images** live in `Assets/StreamingAssets/PropImages/`. `Prop.imagePath` stores only the filename. Use `PropsTool.GetPropImage(prop)` to load a `Texture2D` at runtime.

### Combat targeting rules

- **PlayerA (ranged)** → only hits **EnemyB**. Uses `BulletPool` with `isEnemyLaunched=false`.
- **PlayerB (melee)** → only hits **EnemyA**.
- **All enemies** → hit **all players** (skip dead ones). Enemy bullets use `BulletPool` with `isEnemyLaunched=true`.
- PlayerA and enemies share the same `BulletPool` (differentiated by `Bullet.isEnemyLaunched`).
- `Bullet.TryHitPlayer()` respects `PropOwner` and checks `PlayerCurrentStateController.GetIsDead()` before applying damage.

### Enemy FSM architecture

The Sword module defines a generic FSM:

```
Fsm
 ├── IDataBoard (EnemyDataBoard) — shared state: HP, target player, distance, timers
 ├── IState — OnEnter / OnUpdate / OnFixUpdate / OnExit
 └── State cache — first SwitchState<T> instantiates via factory, subsequent calls reuse
```

Enemy FSM states: `PatrolState` → `ChaseState` → `RangedAttackState` (EnemyA) or `MeleeAttackState` (EnemyB) → `IdleCooldownState` (EnemyB only) → `DeathState`.

`EnemyA` and `EnemyB` inherit from `EnemyBase`, which holds the FSM and data board. Each enemy type has its own ScriptableObject config (`EnemyA_SO`, `EnemyB_SO`) inheriting from `EnemySOBase`, which defines stats plus an `owner` field controlling which player(s) the enemy targets.

### Buff system

- `BuffMgr` (singleton) manages `Dictionary<PropOwner, List<Buff>>`.
- `Buff.AddBuff(target, type, stacks)` auto-stacks or creates. Uses `TaskMgr` (UniTask) for periodic tick callbacks — no per-frame Update.
- `PoisonBuff` ticks MP damage, `BleedBuff` ticks HP damage (with `MinHpThreshold` floor).
- Configs: `BuffConfig` ScriptableObjects (`BuffConfig_Poison.asset`, `BuffConfig_Bleed.asset`).

### Persistence

Three JSON files saved to `Application.dataPath/..` (project root in Editor):

| File | Managed by | Contains |
|---|---|---|
| `players.json` | `JsonData.SavePlayer/LoadPlayer` | Two `PlayerProfile` objects (maxHP, maxMP, bagSize, bag) |
| `warehouse.json` | `JsonData.SaveWarehouse/LoadWarehouse` | Shared warehouse list + property (gold) |
| `dealer.json` | `JsonData.SaveDealer/LoadDealer` | Dealer seed, store list, favoribility |

Saves trigger on every mutation (add/remove/swap items, stat changes). `Newtonsoft.Json` with `TypeNameHandling.All` + `ConstructorHandling.AllowNonPublicDefaultConstructor`.

### Dealer (shop) system

`DealerManager` (static class) uses a deterministic seed derived from date/time (`(Year-1)*12 + (Month-1)*30 + (Day-1)*24 + Hour`) to generate 20 daily items. Higher `favoribility` (raised via `GiftBox` items) increases Rare/Legend drop weights. The store refreshes when the seed changes (new hour).

### Namespace conventions

- `Taffy.OverAllManager` — EventBus, EventList, OverAllSceneManager, OverAllPlayerController
- `Taffy.Data` — Prop, PlayerProfile, PlayerCurrentState, JsonData, PropOccurProbability
- `Taffy.Play.Player` — PlayingHandler_A/B, PlayingTrigger_A/B, PlayerCurrentStateController
- `Taffy.Play.Container` — VarityContainer, ContainerData
- `Taffy.Play.Place` — EvacuateManager
- `Taffy.Home` — WarehouseManager, DealerManager, HomeHandler
- `Taffy.UI` — HomeUIManager, PlayingUIManager
- Sword combat classes use the root namespace (no sub-namespace)
- `SBJC.SBJC_Player_S` — Bullet, BulletPool, player movement/attack

## Code conventions

- **Input**: Uses Unity Input System (`PlayingInputAction` asset). PlayerA/PlayerB action maps are split. Moving, attacking, opening bags/containers, choosing/discarding/replacing props are all input-driven.
- **Step-up movement**: `StepUpMovement.MoveWithStepUp()` (static, in Sword/GamePlay/Common) handles automatic stair/obstacle stepping via BoxCast — used by both players and enemies.
- **Visual root**: Both players and enemies have a `visualRoot` Transform (child GameObject with SpriteRenderer+Animator) that faces the camera in `LateUpdate` while the root collider stays vertical.
- **Attack cooldowns**: Implemented via `TaskMgr.AddTask(() => canAttack = true, cooldownSeconds).Forget()` — UniTask fire-and-forget.
- **`PropOccurProbability.Build()`** uses reflection on `Assembly.GetAssembly(typeof(Prop))` to discover all concrete Prop subclasses and bucket them by rarity. Must be called before any loot generation.
- **Container types**: Named by GameObject convention — if the object is named `Treat_Case`, it becomes a `ContainerType.Cure` container; `Weapon_Case` → Weapon, `Defence_Case` → Armor, `Insurance_Case` → Treasure.

## 沟通偏好（Communication preferences）

- 回答要简洁：最多三个大段（小节），超过就删。不磨叽、不重复说过的内容、不死扣基础概念。
- 但要让初级开发者能看懂：结论先行，说人话，必要时只给一个最小例子。
- 「你冷静一下」= 抛弃无关上下文，只聚焦当前这个小任务，别发散。
