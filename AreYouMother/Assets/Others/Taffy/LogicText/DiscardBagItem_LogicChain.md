# 丢弃背包物品 — 完整逻辑链梳理

生成日期：2026-05-23
覆盖范围：键盘输入 → 输入处理 → 逻辑层 → 数据层 → UI 视觉刷新

---

## 一、按键绑定
文件：`Assets/Others/Taffy/Input/PlayingInputAction.inputactions`

| 玩家 | 打开/关闭背包 | 选择道具（光标移动） | 丢弃当前选中道具 |
|------|-------------|------------------|----------------|
| A    | `E`         | `WASD`（4方向）   | `Q`            |
| B    | `Numpad2`   | 方向键（4方向）    | `Numpad1`      |

背包关闭时 `ChooseProp` 和 `RemoveBagAt` 两个 Action 均处于 Disabled 状态，
只有背包打开后才激活，避免误触。

---

## 二、输入处理层
文件：`Assets/Scripts/Taffy/Play/Player/PlayingHandler_A.cs`
文件：`Assets/Scripts/Taffy/Play/Player/PlayingHandler_B.cs`

### 2-1 打开背包（以 A 为例，B 同理）
```
PlayingHandler_A.OpenOrCloseBag(ctx)
  ├─ isBagClosed = false
  ├─ EnableChooseProp_A()          // Disable Move，Enable ChooseProp + RemoveBagAt，订阅 ChooseProp.performed
  ├─ EnableDiscardProp_A()         // 订阅 RemoveBagAt.performed += RemovePropAt
  └─ OpenBagEvent?.Invoke()        // 通知 UI 层打开背包面板
```

> **A/B 差异**：A 把丢弃订阅单独放在 `EnableDiscardProp_A`；
> B 没有该方法，丢弃订阅直接写在 `EnableChooseProp_B` 里。

### 2-2 丢弃触发
```
玩家按下 Q / Numpad1
  └─ InputSystem 触发 RemoveBagAt.performed
       └─ PlayingHandler_A/B.RemovePropAt(ctx)
            └─ RemovePropAtEvent?.Invoke()   // 无参数，仅通知"要丢弃当前选中的"
```

### 2-3 关闭背包
```
PlayingHandler_A.OpenOrCloseBag(ctx)
  ├─ isBagClosed = true
  ├─ DisableChooseProp_A()         // Enable Move，Disable ChooseProp + RemoveBagAt，取消订阅
  ├─ DisableDiscardProp_A()        // 取消订阅 RemoveBagAt.performed -= RemovePropAt
  └─ CloseBagEvent?.Invoke()
```

---

## 三、逻辑中间层（PlayingUI_pro）
文件：`Assets/Scripts/Taffy/UI/Pro/PlayingUI_pro.cs`

`PlayingUI_pro` 是纯 C# 类（无 MonoBehaviour），负责连接输入层与数据层，
同时维护背包当前光标索引 `propIndex_A/B` 和上一帧索引 `prevPropIndex_A/B`。

### 3-1 事件订阅关系（在 Subscribe() 中建立）
```
handlerA.RemovePropAtEvent  →  PlayingUI_pro.RemoveBagAt_A()
handlerB.RemovePropAtEvent  →  PlayingUI_pro.RemoveBagAt_B()
handlerA.ChoosePropArrowEvent → PlayingUI_pro.GetPropIndex_A(Vector2Int)
handlerB.ChoosePropArrowEvent → PlayingUI_pro.GetPropIndex_B(Vector2Int)
```

### 3-2 RemoveBagAt_A() 执行流程
```
PlayingUI_pro.RemoveBagAt_A()
  ├─ 若背包为空 → return（早返回保护）
  ├─ pcsc.RemovePropFromBagByIndex_A(propIndex_A)   // 写数据
  ├─ 索引夹紧：
  │    count == 0  → propIndex_A = 0
  │    propIndex_A >= count → propIndex_A = count - 1
  ├─ prevPropIndex_A = propIndex_A
  └─ RemoveBagAt_AEvent?.Invoke()                   // 通知 UI 刷新
```

> **B 端缺陷**：`RemoveBagAt_B()` 没有 `Count == 0` 的早返回检查，
> 当背包为空时调用 `RemoveAt` 会抛出 `ArgumentOutOfRangeException`。

---

## 四、数据层
文件：`Assets/Scripts/Taffy/Play/Player/PlayerCurrentStateController.cs`

`PlayerCurrentStateController` 是 Singleton MonoBehaviour，持有两名玩家的
`PlayerCurrentState`（含 `List<Prop> bag`）。

### 4-1 实际调用的方法
```csharp
// 丢弃时调用（来自 PlayingUI_pro）
public void RemovePropFromBagByIndex_A(int index) => playerA.bag.RemoveAt(index);
public void RemovePropFromBagByIndex_B(int index) => playerB.bag.RemoveAt(index);
```

### 4-2 存在但未被调用的方法（死代码）
```csharp
// #region 扔道具 — 当前无任何调用方
public void DiscardProp_A(Prop prop)          => playerA.bag.Remove(prop);
public void DiscardProp_B(Prop prop)          => playerB.bag.Remove(prop);
public void DiscardPropByIndex_A(int index)   => playerA.bag.RemoveAt(index);
public void DiscardPropByIndex_B(int index)   => playerB.bag.RemoveAt(index);
```
与 `RemovePropFromBagByIndex_*` 逻辑完全相同，是重复实现。

---

## 五、UI 视觉层
文件：`Assets/Scripts/Taffy/UI/PlayingUIManager.cs`
UXML：`Assets/Others/Taffy/UI/Bag/BagUI.uxml`
UXML：`Assets/Others/Taffy/UI/PropCase/PropCaseUI.uxml`

### 5-1 事件订阅（在 SubscribeEvents() 中建立）
```
playingUIPro.RemoveBagAt_AEvent  →  RefreshBag_A()
                                 →  CheckingProp_A()
playingUIPro.RemoveBagAt_BEvent  →  RefreshBag_B()
                                 →  CheckingProp_B()
```

### 5-2 RefreshBag_A() — 重建格子列表
```
RefreshBag_A()
  ├─ 从 playingUIPro.GetBag_A() 取当前背包列表
  ├─ BagUI_A.Q("PropsCatalogue").Clear()          // 清空所有格子
  ├─ BagUI_A.Q<Label>("BagInfo").text = ...       // 更新"上限/现存"文本
  └─ foreach prop in bag:
       VisualElement propCase = PropCaseUI.Instantiate()
       propCase.style.backgroundImage = PropsTool.GetPropImage(prop)  // 从 StreamingAssets 读图
       PropsCatalogue.Add(propCase)
```

### 5-3 CheckingProp_A() — 更新选中高亮与道具描述
```
CheckingProp_A()
  ├─ 若背包为空 / 背包已关闭 / propCatalogue_A 为 null → return
  ├─ cur  = playingUIPro.GetPropIndex_A()
  ├─ prev = playingUIPro.GetPrevPropIndex_A()
  ├─ propCatalogue_A.ElementAt(prev).Q("CheckingBackground").style.backgroundColor = null    // 清除旧高亮
  ├─ propCatalogue_A.ElementAt(cur).Q("CheckingBackground").style.backgroundColor  = Color(0.4,0.5,0.8,0.8)  // 新高亮
  ├─ DescribeProp_A()
  │    BagUI_A.Q<Label>("PropName").text    = prop.name
  │    BagUI_A.Q<Label>("PropDescribe").text = "价值/数值/稀有度 + description"
  └─ playingUIPro.SetPrevPropIndex_A(cur)
```

---

## 六、完整调用时序图

```
玩家按 Q / Numpad1
│
▼
InputSystem: RemoveBagAt.performed
│
▼
PlayingHandler_A/B.RemovePropAt()
  └─ RemovePropAtEvent.Invoke()
│
▼
PlayingUI_pro.RemoveBagAt_A/B()
  ├─ PlayerCurrentStateController.RemovePropFromBagByIndex_A/B(index)
  │    └─ playerA/B.bag.RemoveAt(index)          ← 数据写入
  ├─ 索引夹紧
  └─ RemoveBagAt_AEvent/BEvent.Invoke()
│
▼
PlayingUIManager（两个监听同时响应）
  ├─ RefreshBag_A/B()                            ← 重建格子 VisualElement 列表
  └─ CheckingProp_A/B()                          ← 更新高亮 + 底部道具描述文本
```

---

## 七、已知问题与不一致点

| 编号 | 问题 | 位置 | 风险 |
|------|------|------|------|
| 1 | `RemoveBagAt_B()` 缺少 `Count == 0` 早返回 | `PlayingUI_pro.cs:151` | 背包空时丢弃崩溃 |
| 2 | `#region 扔道具` 四个方法与 `RemovePropFromBagByIndex_*` 逻辑重复，无调用方 | `PlayerCurrentStateController.cs:278` | 死代码，维护混乱 |
| 3 | A 的丢弃订阅在 `EnableDiscardProp_A` 单独管理，B 混写在 `EnableChooseProp_B` 中 | `PlayingHandler_A.cs:112` / `PlayingHandler_B.cs:89` | 风格不一致，B 端难以单独控制丢弃权限 |
