# Sword 模块介绍

Sword模块是游戏中的战斗系统核心，包含敌人AI、Buff系统、FSM框架等组件。

## 目录结构

```
Sword/
├── Frames/           # 框架层 - 通用基础设施
├── GamePlay/         # 游戏逻辑层 - 敌人、Buff、投射物
├── SO/               # 数据配置层 - ScriptableObject定义
└── README.md         # 本文件
```

---

## 1. Frames 框架层

### 1.1 Fsm.cs - 有限状态机框架
**职责**：提供通用的FSM实现，驱动游戏对象状态切换

**核心组件**：
- `IState` - 状态接口，定义OnEnter/OnUpdate/OnFixUpdate/OnExit
- `IDataBoard` - 数据黑板接口，用于状态间共享数据
- `Fsm` - 状态机主类，管理状态注册、切换、缓存

**使用方式**：
```csharp
// 1. 创建数据黑板
var board = new EnemyDataBoard(data, transform, this);

// 2. 创建FSM
var fsm = new Fsm(board);

// 3. 注册状态
fsm.AddState<PatrolState>(fsm => new PatrolState(fsm));

// 4. 切换状态
fsm.SwitchState<PatrolState>();

// 5. 更新（在MonoBehaviour.Update中调用）
fsm.FsmUpdate();
```

**设计特点**：
- 使用工厂模式（`Func<Fsm, IState>`）注册状态，避免反射
- 状态缓存机制，状态切换时复用已有实例
- 数据与逻辑分离，通过IDataBoard共享数据

### 1.2 TaskMgr.cs - 异步任务管理器
**职责**：封装UniTask，提供简单的延迟/循环任务接口

**主要方法**：
- `AddTask(Action, float)` - 延迟执行单次任务
- `AddFrameDelay(Action, int)` - 延迟指定帧数执行
- `AddLoopTask(Action, Action, float, int)` - 有限循环任务
- `AddLoopTask(Action, float)` - 无限循环任务

**使用场景**：
- Buff的定时触发
- 技能冷却倒计时
- 延迟播放动画/音效

---

## 2. GamePlay 游戏逻辑层

### 2.1 敌人系统

#### EnemyBase.cs - 敌人基类
**职责**：定义敌人的通用行为和生命周期

**核心方法**：
- `InitFSM()` - 子类重写，初始化状态机
- `OnRangedAttack()` - 远程攻击回调（A类敌人实现）
- `OnMeleeAttack()` - 近战攻击回调（B类敌人实现）
- `TakeDamage(int)` - 受到伤害

#### EnemyDataBoard.cs - 敌人数据黑板
**职责**：存储敌人FSM需要的共享数据

**主要数据**：
- 配置数据（EnemySOBase）
- 运行时数据（HP、目标玩家、距离）
- 游荡数据（出生点、游荡目标、方向）
- 攻击数据（上次攻击时间、是否已攻击）

**关键功能**：
- `UpdateTargetPlayer()` - 自动选择距离最近的玩家作为目标
- `GenerateNextWanderTarget()` - 生成下一个游荡点
- `IsInAggroRange()` - 检查是否在仇恨范围内

#### EnemyA.cs - A类远程敌人
**职责**：实现远程攻击逻辑，发射投射物

**攻击流程**：
1. 进入视野范围 → 切换到RangedAttackState
2. 每3秒发射一次投射物
3. 投射物朝玩家当前位置直线飞行
4. 命中后有20%概率使玩家中毒

**配置依赖**：EnemyA_SO（实现IRemoteAttack接口）

#### EnemyB.cs - B类近战敌人
**职责**：实现近战攻击逻辑，单次攻击后进入冷却

**攻击流程**：
1. 靠近玩家到攻击范围 → 切换到MeleeAttackState
2. 执行一次近战攻击（范围检测）
3. 切换到IdleCooldownState，原地不动
4. 冷却结束后再次追击

**配置依赖**：EnemyB_SO（实现IMeleeAttack接口）

#### EnemyStates.cs - 敌人状态定义
**职责**：定义敌人FSM的所有状态

**状态列表**：
| 状态 | 用途 | 切换条件 |
|------|------|----------|
| PatrolState | 在出生点附近游荡 | 无目标时 |
| ChaseState | 追击玩家 | 发现目标时 |
| RangedAttackState | 远程攻击（A类） | 进入视野范围 |
| MeleeAttackState | 近战攻击（B类） | 进入攻击范围 |
| IdleCooldownState | 攻击后冷却（B类） | 近战攻击后 |
| DeathState | 死亡处理 | HP<=0时 |

#### EnemyA_ChaseState.cs / EnemyB_ChaseState.cs
**职责**：A类和B类敌人的追击状态差异化实现

- **A类**：进入视野就切换到远程攻击（不需要靠近）
- **B类**：需要靠近到攻击范围才攻击

### 2.2 投射物系统

#### Projectile.cs - 敌人发射的投射物
**职责**：处理投射物的飞行和命中逻辑

**行为**：
- 沿直线飞行，有生命周期限制
- 碰撞检测，只击中玩家
- 根据敌人从属关系（PropOwner）过滤目标
- 命中后触发伤害和Buff效果

### 2.3 Buff系统

#### Buff.cs - Buff基类
**职责**：定义Buff的通用行为和生命周期

**核心机制**：
- 使用TaskMgr驱动定时触发（替代Update循环）
- `Start()` - 启动Buff，开始定时触发和倒计时
- `Stop()` - 停止Buff，标记为过期
- `OnTick()` - 子类实现具体效果

#### BuffImpl.cs - Buff具体实现
**职责**：实现中毒和流血两种Buff

**PoisonBuff**：
- 每秒扣除MP
- 可叠加层数
- 持续时间结束后自动移除

**BleedBuff**：
- 每秒扣除HP
- 有最低HP阈值保护（不会扣到1以下）
- 可叠加层数

#### BuffMgr.cs - Buff管理器
**职责**：管理所有玩家的Buff，单例模式

**主要功能**：
- `AddBuff(target, type, stacks)` - 添加Buff（自动叠加或创建新Buff）
- `RemoveBuff(target, type)` - 移除指定Buff
- `RemoveAllBuffs(target)` - 移除玩家所有Buff
- `HasBuff(target, type)` - 检查是否有指定Buff

**叠加规则**：
- 同类型Buff可以叠加层数
- 不超过配置的最大层数
- 叠加时刷新持续时间

---

## 3. SO 数据配置层

### 3.1 EnemySOBase.cs - 敌人数据基类
**职责**：定义敌人的配置数据结构

**核心类**：
- `EnemySOBase` - 基类，包含通用属性
- `EnemyA_SO` - A类敌人数据，实现IRemoteAttack
- `EnemyB_SO` - B类敌人数据，实现IMeleeAttack

**通用属性**：
- atk/hp/moveSpeed - 基础战斗属性
- wanderDistance/wanderHorizontal - 游荡设置
- aggroRadius/loseAggroRadius - 仇恨范围
- owner - 从属关系（A/B/Public）

### 3.2 BuffSOBase.cs - Buff配置
**职责**：定义Buff的配置数据

**BuffConfig属性**：
- BuffType - 类型
- BaseValue - 基础扣减值
- Duration - 持续时间
- TickInterval - 触发间隔
- CanStack/MaxStacks - 是否可叠加/最大层数

---

## 4. 模块交互关系

```
EnemyA/EnemyB (MonoBehaviour)
    │
    ├── 持有 → EnemyDataBoard (IDataBoard)
    │       └── 持有 → EnemySOBase (配置数据)
    │
    ├── 持有 → Fsm
    │       └── 管理 → IState 状态
    │               └── 通过 board 访问共享数据
    │
    └── 触发 → Projectile (远程) / 范围检测 (近战)
            └── 命中后 → BuffMgr.AddBuff()

BuffMgr (单例)
    └── 管理 → Buff 列表
            └── 使用 → TaskMgr 驱动定时触发
```

---

## 5. 使用示例

### 创建A类敌人
```csharp
// 1. 创建SO资源
// Assets/Create/GameSO/EnemyA_Data

// 2. 创建GameObject，添加组件
// - EnemyA 脚本
// - Collider (触发器)
// - Rigidbody

// 3. 配置引用
// - 赋值 EnemyA_Data.asset
// - 设置 firePoint (发射点Transform)
// - 赋值 projectilePrefab (发射物预制体)
```

### 添加Buff
```csharp
// 在投射物命中时
BuffMgr.Instance.AddBuff(PropOwner.A, BuffType.Poison, 1);
```

---

## 6. 扩展指南

### 添加新敌人类型
1. 创建新的SO类继承EnemySOBase
2. 创建新的Enemy类继承EnemyBase
3. 实现InitFSM()注册所需状态
4. 可选：创建新的ChaseState继承ChaseState

### 添加新Buff类型
1. 在BuffType枚举中添加新类型
2. 创建新的Buff类继承Buff
3. 实现OnTick()定义效果
4. 在BuffMgr.CreateBuff()中添加创建逻辑
5. 创建对应的BuffConfig.asset

---

## 7. 注意事项

1. **UniTask依赖**：本模块依赖UniTask包，确保已安装
2. **图层设置**：发射物需要正确的图层配置，避免误碰撞
3. **性能考虑**：大量使用UniTask.Forget()，注意避免内存分配
4. **测试建议**：重点测试双人模式下的仇恨目标切换逻辑
