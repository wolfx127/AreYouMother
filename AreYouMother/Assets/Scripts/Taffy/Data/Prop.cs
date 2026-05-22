
// 道具使用方式
// 1. 中间类（Sword/Armor/Coin/...）规定一类道具共享的字段与接口
// 2. 具体类继承中间类，在无参构造函数里写默认数值；BigSword : Sword 这种"只改数值"的子类只需重写要改的字段
// 3. 实例化：new BigSword()，序列化/反序列化都靠无参构造（Newtonsoft 需要）
//
// 图片：放在 Assets/StreamingAssets/PropImages/ 下，imagePath 只存文件名（含扩展名）
//      运行时调用 PropImageLoader.Load(imagePath) 拿 Sprite

namespace Taffy.Data
{
    /// <summary>
    /// 道具都有归属于的玩家，看看是归属于谁
    /// </summary>
    public enum PropOwner { A, B, Public }
    public enum PropRarity { 普通, 稀有, 传说 }

    // ── 基类 ─────────────────────────────────────────────────────────────────

    public abstract class Prop
    {
        public string     name             = "NullProp";
        public string     description      = "无";
        public string     imagePath        = "NullProp.png";
        public int        value            = 0;
        public int        playingQuantity  = -1;     //-1意味着游戏中数值没用
        public PropRarity rarity           = PropRarity.普通;
        public PropOwner  owner            = PropOwner.Public;
    }

    // ── 接口（playingQuantity 的语义） ────────────────────────────────────────

    public interface IWeapon   { int ATK  { get; } }
    public interface IDefend   { int DefensePower { get; } }
    public interface ITreasure { }

    // ── 默认类 ───────────────────────────────────────────────────────────────

    /// <summary>
    /// 剑
    /// </summary>
    public class Sword : Prop, IWeapon
    {
        public int ATK => playingQuantity;

        public Sword()
        {
            name             = "Sword";
            description      = "普通的剑";
            imagePath        = "sword.png";
            value            = 50;
            playingQuantity  = 10;
        }
    }

    /// <summary>
    /// 护甲
    /// </summary>
    public class Armor : Prop, IDefend
    {
        public int DefensePower => playingQuantity;

        public Armor()
        {
            name             = "Armor";
            description      = "普通的护甲";
            imagePath        = "armor.png";
            value            = 50;
            playingQuantity  = 5;
        }
    }

    /// <summary>
    /// 金币，纯收藏宝物都可以继承它
    /// </summary>
    public class Coin : Prop, ITreasure
    {
        public Coin()
        {
            name        = "金币";
            description = "意味着最小面值";
            imagePath   = "coin.png";
            value       = 1;
        }
    }

    // ── 具体类：只改数值的子类，构造函数里覆盖父类默认值 ─────────────────────

    public class BigSword : Sword
    {
        public BigSword()
        {
            name        = "BigSword";
            description = "更大的剑";
            imagePath   = "big_sword.png";
            value       = 100;
            playingQuantity = 100;
            rarity       = PropRarity.稀有;
        }
    }
}
