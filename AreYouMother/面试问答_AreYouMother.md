# 面试模拟：AreYouMother 项目

> **场景设定**：面试官面前只有你的简历，看不到代码。他根据简历上的每一句话追问技术细节。
> 以下回答基于你的实际代码整理，帮你做面试准备。

---

## 简历原文

> 2026 网易 Mini GameJam 参赛作品。双人共享键盘的俯视角搜刮撤离游戏，融合商人交易、血量成长等 Roguelike 养成玩法。
> - 基于 UI Toolkit + MVP 架构构建背包、主页、战场 UI，实现数据层与视图层分离，优化解耦
> - 综合运用单例、事件总线（观察者）、工厂（敌人状态机）、对象池（射击物/弹幕）等多种设计模式，降低模块耦合，提升运行时性能
> - 道具系统采用组合 + 继承设计，所有道具统一继承 Prop 抽象类，武器、防具、消耗品等分别实现对应接口，便于扩展新道具类型，以及运行时辨识类型用于统一调用
> - 商店刷新基于时间戳哈希生成随机种子，保证同一小时内商品列表一致

---

## Q1：你说 UI 用了 MVP 架构，"数据层与视图层分离"，具体讲讲你的 M / V / P 三层是怎么划分的？举一个具体场景，比如玩家打开背包选中一个道具，数据是怎么从 Model 流到 UI 上的？

**面试官心理**：简历人人都写 MVP/MVC/MVVM，我要听具体细节，看你是真用了还是背名词。

**建议回答**：

我的 MVP 是这样划分的：

- **Model**：`PlayerCurrentStateController`（单例）。它持有对局内两个玩家的所有数据——HP、MP、背包 List、ATK、DEF。它不引用任何 UI 类，完全不知道 UI 的存在。数据变更时通过 C# 的 `event Action` 发出通知（如 `UpdateHP_AEvent`）。

- **View**：`PlayingUIManager`（MonoBehaviour 挂 UIDocument）。它只负责 UI Toolkit 的 `VisualElement` 操作——用 `Q<>()` 查找元素、`Instantiate()` 克隆视觉模板、`Add()`/`Remove()` 挂载 UI、`style.width` 更新血条长度。View 不写任何业务逻辑。

- **Presenter**：`PlayingUI_pro`（纯 C# 类，不继承 MonoBehaviour）。它是中间层——持有 View 引用和 Model 引用，负责"从 Model 取数据、格式化后交给 View 渲染"，以及"接收 View 的用户输入事件、转换成 Model 调用"。

以"打开背包选中道具"为例：

```
玩家按方向键
  → PlayingHandler_A (Input层) 发出 ChoosePropArrowEvent
  → PlayingUI_pro.ObtainPropIndex_A() 接收，做光标索引的网格换算（5列布局的行列映射）
  → 更新内部索引 propIndex_A，触发 CheckingProp_AEvent
  → PlayingUIManager.CheckingProp_A() 响应：
      1. 取消上一个格子的蓝色背景
      2. 给当前格子上蓝色背景
      3. 调用 Presenter 的 GetCurrentPropName_A() 拿到道具名
      4. 写到 UI 的 Label 上
```

我承认这个 MVP 不完美——View 直接 `new PlayingUI_pro()` 创建 Presenter，没有通过接口注入。但在 48 小时 GameJam 的时间限制下，这个方案有效控制了 View 文件的代码量（否则所有逻辑堆在 MonoBehaviour 里会到上千行），也让我在后续加功能时（比如用道具效果）只需改 Presenter 不改 View 结构。

---

## Q2：你说用了事件总线来实现观察者模式。讲讲你是怎么实现的？和 C# 原生的 `event` 或者 Unity 的 `UnityEvent` 比，你的方案有什么优劣？

**面试官心理**：事件总线是经典面试题。我要看你是真的自己写了一个，还是把 UnityEvent 改个名就叫事件总线了。

**建议回答**：

我自己实现了一个全局事件总线 `EventBus`（静态类，在 `Taffy.OverAllManager` 命名空间下）。核心设计：

**类型即频道**：每种事件定义为一个 struct，struct 的类型本身就是频道的 key。比如：

```csharp
// 定义事件（在 EventList.cs 里）
public struct Evacuate_AEvent { }  // 空 struct，只表示"发生了"
public struct GetPlayersInfosEvent {
    public int HP_playerA;
    public int HP_playerB;
    // ... 带数据的 struct
}

// 订阅
EventBus.Subscribe<Evacuate_AEvent>(OnEvacuate_A);

// 发布
EventBus.Publish(new Evacuate_AEvent());
```

内部用 `Dictionary<Type, Delegate>` 存储，`Subscribe` 时 `Delegate.Combine`，`Publish` 时 `Delegate.Invoke`。

**和原生 C# event 的对比**：

| | 我的 EventBus | C# event | UnityEvent |
|---|---|---|---|
| 跨模块通信 | 天然解耦，发布者和订阅者互相不引用 | 订阅者必须持有发布者引用 | 同上，且只能在 Inspector 拖拽 |
| GC 分配 | Publish 时零分配（struct 约束） | 无 | 有装箱 |
| 类型安全 | 编译期检查 | 编译期检查 | 运行时检查 |
| 可追踪性 | 差——难以静态分析谁订阅了谁 | 好——调用链清晰 | 中等 |

**实际使用中最大的收益**：撤离系统。玩家 A 走到撤离点按按钮 → `EventBus.Publish(new Evacuate_AEvent())` → `EvacuateManager` 收到 → 判断 A 和 B 都撤离则结算。如果不用事件总线，`PlayingHandler_A` 需要持有 `EvacuateManager` 的引用，而这两个类在语义上不应该互相感知。

**一个坑**：同一个 handler 重复 Subscribe 不会去重。我靠"在 `OnEnable` 里 Subscribe，`OnDisable` 里 Unsubscribe 成对出现"的纪律来保证，但缺乏框架层面的防护。

---

## Q3：简历提到"单例模式"，你都用在哪了？单例在场景切换时生命周期怎么管理的？有没有遇到过"单例已经销毁但其他模块还在调用"的情况？

**面试官心理**：Unity 里滥用单例是通病，我要看你对生命周期有没有意识。

**建议回答**：

项目中用了这些单例：

| 单例 | 所在场景 | 生命周期 |
|---|---|---|
| `OverAllPlayerController` | Start（常驻场景） | 整个游戏生命周期，存玩家属性上限和背包 |
| `PlayerCurrentStateController` | Play（对局场景） | 仅对局内有效，存 HP/MP/临时 ATK |
| `BuffMgr` | Play | 对局内 |
| `BulletPool` | Play | 对局内 |
| `EvacuateManager` | Play | 对局内 |
| `HomeHandler` | Home | 仅 Home 场景 |

**场景切换时的生命周期**：我的场景架构是 Start 场景常驻 + Home/Play 场景 Additive 加载。所以流程是：

```
Start 场景加载
  → OverAllPlayerController.Awake() 把自己设为 Instance
  → 加载 Home 场景，卸载 Start 不卸载自己（因为它在 Start 里）

切换到 Play：
  → Home 场景卸载 → HomeHandler.Instance 随场景销毁
  → Play 场景加载 → PlayerCurrentStateController / BuffMgr / BulletPool 重新 Awake，重新赋值 Instance
```

关键点：**跨场景的单例（`OverAllPlayerController`）存在常驻场景中**，对局内单例随场景销毁重建。这样避免了"切场景后单例引用悬空"的问题。

**但有一个真实风险**：`PlayerCurrentStateController` 的 `Awake` 里做了 `if (Instance != null) Destroy(gameObject)`，防止重复。但如果有人在对局内把常驻场景的单例误引用到对局单例上——比如 `OverAllPlayerController.Instance` 的某些方法在 `PlayerCurrentStateController` 还没 `Awake` 时就被调用——就会 NullReference。我在 `OverAllSceneManager` 里通过异步加载 + `InitialPlayingSceneEvent` 的时序保证了对局单例初始化完成后才传递数据，但这个时序依赖比较脆弱。

---

## Q4：状态机的"工厂模式"具体怎么实现？为什么不直接用 Unity 的 Animator 做状态管理？

**面试官心理**：Animator 也能做状态机，你额外造轮子的理由是什么。

**建议回答**：

我的 FSM 在 `Sword/Frames/Fsm.cs`，设计要点：

**注册不实例化**：外部用 `fsm.AddState<ChaseState>(fsm => new ChaseState(fsm))` 注册——传入的是一个工厂委托（`Func<Fsm, IState>`），不是状态实例。FSM 把它存进 `Dictionary<Type, Func<Fsm, IState>>`，此时还没有 new。

**首次切换才实例化**：`SwitchState<ChaseState>()` 被调用时，先检查工厂字典有没有注册，再检查缓存字典有没有已有实例。如果没有，调用工厂委托 new 出来，并缓存到 `Dictionary<Type, IState>`。之后每次切到这个状态都复用已有实例，零分配，零反射。

**不用 Animator 的原因**：

1. **Animator 做逻辑状态机很痛苦**。它的状态切换靠 trigger/bool/float 参数，逻辑（比如"进入追击范围 → 切追击状态"）得写在 `MonoBehaviour.Update` 里读 Animator 参数，代码分散。我的 FSM 让每个状态有独立的 `OnEnter/OnUpdate/OnExit`，逻辑内聚在一个类里。

2. **数据共享**。我通过 `IDataBoard`（`EnemyDataBoard`）在状态间共享数据（HP、目标玩家、距离、上次攻击时间），不需要每个状态自己去 GetComponent 或找引用。

3. **Animator 会引入动画状态**。敌人有动画（Idle/Move/Attack/Death），但这些动画参数通过 `_animator.SetFloat("Speed", ...)` 在 FSM 状态的 `OnUpdate` 里设置——动画播放和逻辑状态是分开的，逻辑 FSM 决定"现在该做什么"，动画 Animator 只负责"看起来像什么"。

简单说：**Animator 管表现，Fsm 管逻辑**，各司其职。

---

## Q5：对象池你怎么实现的？池的大小怎么定的？如果子弹池满了但还需要发射新子弹，会发生什么？

**面试官心理**：对象池说起来简单，但边界情况处理能看出水平。

**建议回答**：

`BulletPool` 是一个 MonoBehaviour 单例，内部用 `Queue<GameObject>`：

- `Awake` 时预实例化 `poolSize = 40` 颗子弹，`SetActive(false)` 入队
- `Get()` 时从队列 Dequeue 一颗，`SetActive(true)` 返回
- 子弹在命中/超距离后自己调用 `Pool.Back(gameObject)`，内部调 `ResetBullet()` 清速度并 `SetActive(false)` 重新入队
- 容量上限 `maxPoolSize = 80`

**池大小的考虑**：不是精确计算出来的，是基于估算——子弹飞行 15m/s，最大射程 50m，存活约 3.3 秒。PlayerA 冷却 0.5 秒 + 3-5 个远程敌人每 3 秒发射一次，稳态大约 15-25 颗子弹同时存在。40 预创建 + 80 上限给了足够余量。

**池满了的情况**：当前代码池满时返回 null，调用方（`PlayingTrigger_A.GetAttackEnemies()`）的判断是 `if (bulletGo == null) return;` ——攻击动画播了、冷却走了，但子弹没生成。**说实话这是当前设计的一个未处理边界**，在测试中没触发过（因为敌人数量少），但如果后期策划加弹幕 BOSS 就会暴露。

**如果现在改进**，我会：
- 给每颗子弹加 `maxLifetime`（如 5 秒），超时自动回收，不单纯依赖碰撞和距离
- 池满时做 LRU 淘汰（回收最老的那颗活跃子弹），保证新子弹总能发射
- 在 `Get()` 失败前主动遍历活跃子弹做一次清理

---

## Q6：道具系统你说的"组合 + 继承"，能具体展开吗？假设我现在要做一把"每次攻击附带吸血效果的新弓箭"，你要改哪些文件、怎么写代码？

**面试官心理**：经典扩展性测试题。要看你设计的时候有没有给扩展留空间。

**建议回答**：

道具系统的设计分层是：

```
Prop (抽象基类)            ← 定义 name/description/value/owner/rarity 等通用字段
  ├── Sword : Prop, IWeapon              ← 武器接口给 ATK
  ├── Bow : Prop, IWeapon, IRemoteAttack ← 远程武器多一个发射方法
  ├── Armor : Prop, IDefend              ← 防具接口给 DEF
  ├── CurePotion : Prop, ICure, IUsable  ← 消耗品，能使用
  └── Coin : Prop, ITreasure             ← 宝物，纯收藏卖钱
```

接口的语义：
- `IWeapon` → 有攻击力，`AssignATK()` 把攻击力赋值给玩家
- `IDefend` → 有防御力
- `ICure` → 有治疗量
- `ICultivate` → 永久成长类（加 HP 上限、加商人好感度）
- `IUsable` → 一次性使用（喝药水、使用魔法镜瞬移）
- `ITreasure` → 无战斗效果，纯卖钱标记
- `IRemoteAttack` → 远程发射方法（弓用）

**做"吸血弓箭"的做法**：

1. **如果吸血是 Buff 效果**（命中后给敌人挂 Debuff，每跳伤害转化为回血）：不需要改现有类。弓箭命中 → 敌人受伤的逻辑已经在 `Bullet.OnTriggerEnter` 里处理。在命中后通过 `BuffMgr.Instance.AddBuff(target, BuffType.LifeSteal, 1)` 加一个新 Buff 即可。需要在 `BuffImpl.cs` 里新写一个 `LifeStealBuff : Buff`，在 `OnTick()` 里扣敌人血并调用 `PlayerCurrentStateController.Instance.Cure_A(amount)` 回血。

2. **如果吸血是弓本身的被动属性**：创建 `VampireBow : Bow`，只改构造函数里的 `playingQuantity` 和 `description`，吸血逻辑通过新增 `ILifeSteal` 接口 + 在 `PlayingTrigger_A.GetAttackEnemies()` 里判断武器是否实现了 `ILifeSteal` 来处理。

实际代码中已经有好几个"只改默认值的子类"例子——`BigSword : Sword` 只在构造函数里把 `name` 改成"大剑"、`playingQuantity` 改成 100，其他全继承。新增道具类型的成本很低。

**关于序列化**：我用 Newtonsoft 的 `TypeNameHandling.All`，JSON 里会存 `"$type": "Taffy.Data.VampireBow, Assembly-CSharp"`。这有一个隐患——如果后续重命名类或移动命名空间，老存档里的类型名就找不到了。GameJam 阶段没做迁移方案，但商业项目中应该用自定义 `ISerializationBinder` 做类型名映射表。

---

## Q7：商店"基于时间戳哈希生成随机种子，保证同一小时内商品列表一致"——这个哈希具体怎么算的？如果玩家把系统时间调到一小时后，商店会刷新吗？

**面试官心理**：算法实现细节 + 安全性/边界考虑。

**建议回答**：

种子计算公式：

```csharp
seed = (年份 - 1) * 12 + (月份 - 1) * 30 + (日期 - 1) * 24 + 当前小时
```

这个公式把年月日时编码成一个整数，每个小时 seed 不同。然后用这个 seed 做确定性随机——对商店的 20 个槽位，用 `Hash(seed, slotIndex)` 产生随机数决定稀有度（Common/Rare/Legend），再根据稀有度从对应池子里用 `Hash(seed, slotIndex + 20)` 决定具体道具。

**同一小时商品一致**是因为：seed 在同一小时内不变 → 两次调用 `GetStore()` 用同样的 seed → 同样的 Hash 函数 → 同样的结果。

**改系统时间确实会刷新商店**。因为是 `DateTime.Now` 取本地时间，不是服务端时间。在单机 GameJam 游戏里这不算问题（玩家本来就可以自由游玩），但如果有排行榜或联机功能就需要服务端时间戳。

**一个细节**：商店刷新后还会 `JsonData.SaveDealer()` 写盘。所以如果玩家把时间调到未来 → 商店刷新 → 买了东西 → 调回正常时间 → seed 变了 → 商店再次刷新。这个行为在代码里是自然的，但可能不是设计意图——当时没考虑防时间回溯。

---

## Q8：双人共享键盘，Input System 怎么区分两个玩家的输入？如果两个玩家同时按键（比如同时按攻击），会有冲突吗？

**面试官心理**：本地多人游戏的输入设计是一个值得展开的技术点。

**建议回答**：

我用的是 Unity Input System（`com.unity.inputsystem`），定义了一个 `PlayingInputAction` 资产，里面分了两个 Action Map——`PlayerA` 和 `PlayerB`，每个 Map 绑定不同的按键：

- PlayerA：WASD 移动、F 交互、Z 使用道具、Space 攻击……
- PlayerB：方向键移动、小键盘 0 交互、小键盘 9 使用道具、小键盘 Enter 攻击……

两个玩家完全独立的 Input Action 绑定，不共享按键，所以**不会冲突**。每个玩家的 `PlayingHandler` 在 `OnEnable` 里 enable 自己的 Action Map，在 `OnDisable` 里 disable。

**但有一个设计上的取舍**：当前只支持键盘，没有手柄支持。如果要加手柄，Input System 的 Action Map 体系天然支持——给 PlayerA 绑定键盘+手柄1，PlayerB 绑定另一套键盘+手柄2 即可，代码逻辑不用动。

---

## Q9：你的数据（玩家属性、背包、仓库、商店）是怎么持久化的？如果游戏在一个对局中途崩溃了，玩家的道具会丢吗？

**面试官心理**：持久化策略 + 异常安全性。这是游戏开发的常见坑。

**建议回答**：

数据持久化用 Newtonsoft.Json 序列化到三个 JSON 文件（存在 `Application.dataPath` 上级目录，也就是项目根目录）：

| 文件 | 内容 |
|---|---|
| `players.json` | 两个玩家的 maxHP/maxMP/bagSize/背包道具列表 |
| `warehouse.json` | 共享仓库道具列表 + 总资产金币数 |
| `dealer.json` | 商店 seed + 当前商品列表 + 商人好感度 |

Home 场景中（仓库、商店、背包管理）每次操作后立即写盘——例如从仓库取出道具，会调 `JsonData.SavePlayer()` + `JsonData.SaveWarehouse()`。所以 Home 里的数据是实时持久化的。

**但对局中会丢**。对局内玩家的 HP/MP 变化、捡到的道具，全程只操作内存中的 `PlayerCurrentStateController`，没有写盘。持久化发生在撤离/死亡结算时——`GiveBags()` 把对局背包回传给常驻的 `OverAllPlayerController`，后者调 `JsonData.SavePlayer()` 写盘。

所以如果对局中途崩溃：道具全丢，回到对局前的状态。**但这其实是玩法设计的一部分**——"撤离失败 = 失去一切"是很多 Roguelike/Extraction 游戏的规则，不是 bug。

不过客观讲，如果换个游戏类型（比如不能接受进度丢失的 RPG），就需要在对局内做增量存档或定期自动保存。

---

## Q10：这个项目你做了 48 小时，你觉得自己做得最好的一个技术决策是什么？如果再来一次，你会改一个什么东西？

**面试官心理**：考察自我反思能力和技术判断力。

**建议回答**：

**做得最好的决策**：用 struct 类型作为事件频道 key 的 EventBus 设计。它同时解决了三个问题——（1）不需要任何字符串 key，编译器帮你检查事件名；（2）Publish 时零 GC，适合高频调用（比如对局内的状态同步）；（3）扩展新事件只需加一个 struct，不需要改任何注册代码。整个项目里场景切换、撤离结算、箱子交互、道具传递全部走 EventBus，没有一条硬引用。

**最想改的**：防御性编程做得不够。几个例子：
- 对象池满了只打 Warning 不处理，返回 null
- FSM 重复注册同名状态静默覆盖，不报错
- 道具序列化用了 `TypeNameHandling.All` 写类型全名进 JSON，类改名就炸存档，却没有迁移方案
- `PropsTool.GetPropImage()` 每次调用都从磁盘 `File.ReadAllBytes` 读图片，在刷新背包时会高频触发

如果再来一次，我会给每个系统加更严格的边界检查，以及至少给序列化做一个类型名映射表兜底。

---

## 面试官总结（供你参考面试官可能的评价）

**加分项**：
- 短时间内搭出完整可玩的游戏，且架构分层清晰（战斗框架/游戏逻辑/UI/持久化各层职责明确）
- EventBus 和 FSM 的设计有独立思考，不是照搬 Asset Store 插件
- 道具系统的接口组合设计体现了 OOP 抽象能力——`IWeapon`/`IDefend`/`ICure`/`ICultivate`/`IUsable` 每种行为都有独立接口
- 能讲清楚自己做的好决策和不足之处，有反思意识

**需要补的**：
- 防御性编程习惯（边界检查、错误处理）
- 性能敏感路径的 profiling 意识（磁盘 IO 不应该出现在 UI 刷新循环里）
- 可测试性——当前代码没有单元测试，架构上也没预留测试接口

**总体判断**：合适的初级~实习岗位候选人，有潜力，需要 mentor 带一带工程规范。
