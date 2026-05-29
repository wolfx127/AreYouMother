# AreYouMother 架构图

## 1. 场景流转

```mermaid
flowchart LR
    Start场景 -->|"任意键"| Constant场景
    Constant场景 -->|"Awake: 加载"| Home场景
    Home场景 -->|"EventBus: ChangeSceneHomeToPlayingEvent"| Play场景
    Play场景 -->|"EventBus: InitialPlayingSceneEvent"| 初始化对局状态
```

---

## 2. 整体模块依赖（顶层）

```mermaid
flowchart TD
    subgraph OverAll["OverAllManager（全局）"]
        EventBus
        OverAllSceneManager
        OverAllPlayerController
        PropsTool
    end

    subgraph Data["Data（数据定义）"]
        Prop及子类
        PropOccurProbability
        PlayerProfile
        ContainerData
        JsonData
    end

    subgraph Home["Home（主城）"]
        HomeHandler
        HomeUI_pro
        HomeUIManager
        WarehouseManager
        DealerManager
    end

    subgraph Play["Play（对局）"]
        PlayerCurrentStateController
        PlayingHandler_A_B["PlayingHandler_A/B"]
        PlayingTrigger_A_B["PlayingTrigger_A/B"]
        VarityContainer
    end

    subgraph PlayUI["Play UI"]
        PlayingUI_pro
        PlayingUIManager
    end

    subgraph Sword["Sword（敌人/Buff框架）"]
        Fsm
        TaskMgr
        EnemyBase
        EnemyA_B["EnemyA/B"]
        BuffMgr
        Projectile
    end

    OverAll --> Data
    OverAll --> Home
    OverAll --> Play
    Home --> Data
    Play --> Data
    Play --> OverAll
    PlayUI --> Play
    PlayUI --> Data
    Sword --> Data
    Sword --> Play
```

---

## 3. Home 场景：数据流与事件流

```mermaid
flowchart TD
    Input["键盘输入 (InputSystem)"]
    HomeHandler
    HomeUI_pro
    HomeUIManager
    WarehouseManager
    DealerManager
    OverAllPlayerController
    JsonData

    Input -->|"ChooseProp_A/BEvent\nReplaceProp_A/BEvent\nUseProp_A/BEvent"| HomeHandler
    HomeHandler --> HomeUI_pro

    HomeUI_pro -->|"读写背包"| OverAllPlayerController
    HomeUI_pro -->|"读写仓库"| WarehouseManager
    HomeUI_pro -->|"读写商店"| DealerManager

    HomeUI_pro -->|"CheckProp_A/BEvent\nRefreshBag/Warehouse/DealerEvent\nUpdatePropertyEvent"| HomeUIManager

    OverAllPlayerController -->|"SavePlayer"| JsonData
    WarehouseManager -->|"SaveWarehouse"| JsonData
    DealerManager -->|"SaveDealer"| JsonData
    JsonData -->|"Load*（启动时）"| WarehouseManager
    JsonData -->|"Load*（启动时）"| DealerManager
    JsonData -->|"LoadPlayer（启动时）"| OverAllPlayerController

    HomeUIManager -->|"渲染 UI Toolkit"| UIView["UI 视图\n(背包/仓库/商人格)"]
```

---

## 4. Play 场景：数据流与事件流

```mermaid
flowchart TD
    InputA["键盘输入 A (WASD等)"]
    InputB["键盘输入 B (方向键等)"]
    HandlerA["PlayingHandler_A"]
    HandlerB["PlayingHandler_B"]
    TriggerA["PlayingTrigger_A"]
    TriggerB["PlayingTrigger_B"]
    ContainerData
    VarityContainer
    PCSC["PlayerCurrentStateController"]
    pro["PlayingUI_pro"]
    UIManager["PlayingUIManager"]
    EventBus

    InputA --> HandlerA
    InputB --> HandlerB

    HandlerA -->|"OpenBag/Container\nChoosePropArrow\nDiscard/Replace\nEvacuate"| pro
    HandlerB -->|"同上"| pro

    TriggerA -->|"GiveContainer_AEvent"| EventBus
    TriggerB -->|"GiveContainer_BEvent"| EventBus
    EventBus -->|"GiveContainer_*Event"| pro

    VarityContainer -->|"Start: 填充"| ContainerData
    ContainerData -->|"被持有"| pro

    pro -->|"读写背包/箱子"| PCSC
    pro -->|"CheckingProp_A/BEvent\nDiscard/ReplacePropEvent"| UIManager
    PCSC -->|"UpdateHP/MPEvent"| UIManager

    UIManager -->|"渲染 UI Toolkit"| UIView["UI 视图\n(血条/背包/箱子格)"]
```

---

## 5. Sword 敌人系统（内部）

```mermaid
flowchart TD
    EnemySO["EnemyA_SO / EnemyB_SO\n(ScriptableObject)"]
    EnemyBase
    EnemyA
    EnemyB
    DataBoard["EnemyDataBoard (IDataBoard)"]
    Fsm
    States["PatrolState / ChaseState\nRangedAttackState / MeleeAttackState\nIdleCooldownState / DeathState"]
    Projectile
    BuffMgr
    BuffConfig["BuffConfig (SO)"]
    Buff["PoisonBuff / BleedBuff"]
    TaskMgr

    EnemySO -->|"注入数据"| EnemyBase
    EnemyBase --> DataBoard
    EnemyBase --> Fsm
    EnemyA -->|"继承"| EnemyBase
    EnemyB -->|"继承"| EnemyBase

    Fsm -->|"驱动"| States
    States -->|"读取"| DataBoard
    DataBoard -->|"检测最近玩家"| HandlerAB["PlayingHandler_A/B\n(获取 Transform)"]

    EnemyA -->|"OnRangedAttack: 生成"| Projectile
    EnemyB -->|"OnMeleeAttack: OverlapSphere"| HandlerAB

    Projectile -->|"命中后 TODO: EventBus 扣血"| PCSC_TODO["PlayerCurrentStateController\n(暂 Debug.Log)"]
    Projectile -->|"TryApplySpecialEffect"| BuffMgr

    BuffMgr --> BuffConfig
    BuffMgr -->|"创建/管理"| Buff
    Buff -->|"TickLoop"| TaskMgr
    Buff -->|"TODO: EventBus 扣血/扣蓝"| PCSC_TODO
```

---

## 6. 数据持久化

```mermaid
flowchart LR
    OPC["OverAllPlayerController"]
    WM["WarehouseManager"]
    DM["DealerManager"]
    JD["JsonData\n(Newtonsoft.Json)"]
    Files["players.json\nwarehouse.json\ndealer.json\n(项目根目录)"]

    OPC -->|"任意背包写操作 / ExitGameEvent"| JD
    WM -->|"任意写操作"| JD
    DM -->|"任意写操作"| JD
    JD <-->|"读/写"| Files
    JD -->|"启动时 Load*"| OPC
    JD -->|"启动时 Load*"| WM
    JD -->|"启动时 Load*"| DM
```

---

## 关键设计说明

| 层 | 职责 | 说明 |
|---|---|---|
| `EventBus` | 全局解耦消息总线 | struct 泛型，零 GC |
| `*_pro` 类 | 业务逻辑与 UI 解耦层 | `HomeUI_pro` / `PlayingUI_pro` 不继承 MonoBehaviour，持有数据引用，暴露事件给 Manager |
| `*Manager` (MonoBehaviour) | 纯视图层 | 只渲染 UI Toolkit，订阅 pro 层事件 |
| `*Manager` (static) | 数据管理 | `WarehouseManager` / `DealerManager`，静态单例持有 List<Prop> |
| `Sword` 模块 | 独立 AI/Buff 框架 | 仅通过 `PropOwner` 和 `PlayingHandler` 与 Taffy 耦合；Buff 伤害尚未接入 EventBus（TODO） |
