using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.CompilerServices;
using Taffy.Data;
using Taffy.Home;
using Taffy.OverAllManager;
using Taffy.Play.Container;
using Taffy.Play.Player;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

/// <summary>
/// 道具引用查看器（UI Toolkit 版）：进入 Play 模式后，从项目所有"已知数据入口"出发做一次对象图遍历，
/// 列出每个 Prop 对象被谁、经过哪条路径引用着。
/// 打开方式：菜单 Tools → Prop 引用查看器。
/// </summary>
public class PropReferenceViewer : EditorWindow
{
    [MenuItem("Tools/Prop 引用查看器")]
    public static void Open()
    {
        var w = GetWindow<PropReferenceViewer>("Prop 引用查看器");
        w.Show();
    }

    /// <summary>按引用比较（不能用默认比较器，防止某些类型重写了 Equals）</summary>
    sealed class RefComparer : IEqualityComparer<object>
    {
        public static readonly RefComparer Instance = new RefComparer();
        public new bool Equals(object x, object y) => ReferenceEquals(x, y);
        public int GetHashCode(object obj) => RuntimeHelpers.GetHashCode(obj);
    }

    class FoundProp
    {
        public Prop prop;
        public readonly List<string> paths = new List<string>();
        public string Label => prop == null ? "(null)" : $"{prop.name} [{prop.owner}]";
    }

    const int MaxDepth = 12;
    const BindingFlags Flags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;

    readonly Dictionary<Prop, FoundProp> found = new Dictionary<Prop, FoundProp>(RefComparer.Instance);

    Label hint;
    Button playBtn;
    Button scanBtn;
    ScrollView propList;
    ScrollView pathList;

    void CreateGUI()
    {
        var root = rootVisualElement;
        root.style.paddingTop = root.style.paddingBottom = 8;
        root.style.paddingLeft = root.style.paddingRight = 8;

        hint = new Label("需要进入 Play 模式：背包、仓库、商店等数据都在运行时存在。");
        hint.style.whiteSpace = WhiteSpace.Normal;
        root.Add(hint);

        var row = new VisualElement();
        row.style.flexDirection = FlexDirection.Row;
        playBtn = new Button(() => EditorApplication.EnterPlaymode()) { text = "进入 Play 模式" };
        scanBtn = new Button(Scan) { text = "扫描所有 Prop 引用" };
        row.Add(playBtn);
        row.Add(scanBtn);
        root.Add(row);

        propList = new ScrollView();
        propList.style.flexGrow = 1;
        root.Add(propList);

        var title = new Label("引用路径：");
        root.Add(title);

        pathList = new ScrollView();
        pathList.style.flexGrow = 1;
        root.Add(pathList);

        UpdatePlayUI();
        EditorApplication.playModeStateChanged += PlayModeChanged;
    }

    void OnDisable()
    {
        EditorApplication.playModeStateChanged -= PlayModeChanged;
    }

    void PlayModeChanged(PlayModeStateChange state)
    {
        if (state == PlayModeStateChange.ExitingPlayMode)
        {
            // 退出 Play 后运行时对象全没了，清掉旧结果防止点到悬空引用
            found.Clear();
            propList.Clear();
            pathList.Clear();
        }
        UpdatePlayUI();
    }

    void UpdatePlayUI()
    {
        bool playing = EditorApplication.isPlaying;
        hint.visible = !playing;
        playBtn.visible = !playing;
        scanBtn.SetEnabled(playing);
    }

    void Scan()
    {
        found.Clear();
        pathList.Clear();

        // 已知根：项目里所有可能持有 Prop 的数据入口（新入口以后加在这里）
        var seen = new HashSet<object>(RefComparer.Instance);
        var roots = new List<(string name, object obj)>();
        AddRoot(roots, seen, "OverAllPlayerController", OverAllPlayerController.Instance);
        AddRoot(roots, seen, "PlayerCurrentStateController", PlayerCurrentStateController.Instance);
        AddRoot(roots, seen, "WarehouseManager.warehouse", WarehouseManager.GetWarehouse());
        AddRoot(roots, seen, "DealerManager.store", DealerManager.GetStore());

        foreach (var c in FindObjectsByType<ContainerData>(FindObjectsSortMode.None))
            AddRoot(roots, seen, c.name, c);

        // 聪明法：扫程序集，找出所有"字段里含 Prop"的 MonoBehaviour，场景里的实例也当根
        foreach (var t in FindPropHolderTypes())
            foreach (var mb in FindObjectsByType(t, FindObjectsSortMode.None))
                AddRoot(roots, seen, $"{mb.name} ({t.Name})", mb);

        foreach (var (name, obj) in roots)
            Walk(obj, name, new HashSet<object>(RefComparer.Instance), 0);

        RebuildPropList();
        Debug.Log($"[PropViewer] 扫描完成，共找到 {found.Count} 个道具对象。");
    }

    void RebuildPropList()
    {
        propList.Clear();
        foreach (var f in found.Values)
        {
            var btn = new Button(() => ShowPaths(f)) { text = $"{f.Label} —— {f.paths.Count} 处引用" };
            propList.Add(btn);
        }
    }

    void ShowPaths(FoundProp f)
    {
        pathList.Clear();
        var nameLabel = new Label(f.Label);
        pathList.Add(nameLabel);
        foreach (var p in f.paths)
            pathList.Add(new Label("• " + p));
    }

    static void AddRoot(List<(string, object)> roots, HashSet<object> seen, string name, object obj)
    {
        if (obj == null || !seen.Add(obj)) return;
        roots.Add((name, obj));
    }

    static List<Type> propHolderTypes;

    /// <summary>扫一遍所有程序集，找出"实例字段里含 Prop"的 MonoBehaviour 类型（结果缓存）</summary>
    static List<Type> FindPropHolderTypes()
    {
        if (propHolderTypes != null) return propHolderTypes;
        propHolderTypes = new List<Type>();
        foreach (var asm in AppDomain.CurrentDomain.GetAssemblies())
        {
            Type[] types;
            try { types = asm.GetTypes(); } catch (Exception) { continue; }
            foreach (var t in types)
            {
                if (t == typeof(MonoBehaviour) || !typeof(MonoBehaviour).IsAssignableFrom(t)) continue;
                if (HasPropField(t)) propHolderTypes.Add(t);
            }
        }
        return propHolderTypes;
    }

    static bool HasPropField(Type t)
    {
        foreach (var f in t.GetFields(Flags))
        {
            if (f.IsStatic) continue;
            if (TypeMayHoldProp(f.FieldType)) return true;
        }
        return false;
    }

    static bool TypeMayHoldProp(Type t)
    {
        if (t == typeof(Prop)) return true;
        if (t.IsArray) return TypeMayHoldProp(t.GetElementType());
        if (t.IsGenericType)
        {
            foreach (var arg in t.GetGenericArguments())
                if (TypeMayHoldProp(arg)) return true;
        }
        return false;
    }

    void Walk(object obj, string path, HashSet<object> visited, int depth)
    {
        if (obj == null || depth > MaxDepth) return;
        if (!visited.Add(obj)) return;

        if (obj is Prop p)
        {
            if (!found.TryGetValue(p, out var fp))
            {
                fp = new FoundProp { prop = p };
                found[p] = fp;
            }
            fp.paths.Add(path);
            return; // 道具内部不再下钻
        }

        if (obj is IEnumerable en)
        {
            int i = 0;
            foreach (var item in en)
                Walk(item, $"{path}[{i++}]", visited, depth + 1);
            return;
        }

        foreach (var f in obj.GetType().GetFields(Flags))
        {
            if (f.IsStatic) continue;          // 不追静态字段，防止兜圈
            if (ShouldSkip(f.FieldType)) continue;
            object v;
            try { v = f.GetValue(obj); } catch { continue; }
            if (v == null) continue;
            Walk(v, $"{path}.{f.Name}", visited, depth + 1);
        }
    }

    /// <summary>决定哪些类型不往下钻：基础类型、委托，以及 Transform/Texture 这类原生对象</summary>
    static bool ShouldSkip(Type t)
    {
        if (t.IsPrimitive || t.IsEnum || t == typeof(string) || t == typeof(decimal)) return true;
        if (t == typeof(Type) || typeof(MulticastDelegate).IsAssignableFrom(t)) return true;
        if (typeof(UnityEngine.Object).IsAssignableFrom(t))
        {
            // 只钻我们自己挂数据的 MonoBehaviour / ScriptableObject，不钻原生对象
            return !(typeof(MonoBehaviour).IsAssignableFrom(t) || typeof(ScriptableObject).IsAssignableFrom(t));
        }
        return false;
    }
}
