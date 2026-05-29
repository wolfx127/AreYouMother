
// 道具使用方式
// 1. 中间类（Sword/Armor/Coin/...）规定一类道具共享的字段与接口
// 2. 具体类继承中间类，在无参构造函数里写默认数值；BigSword : Sword 这种"只改数值"的子类只需重写要改的字段
// 3. 实例化：new BigSword()，序列化/反序列化都靠无参构造（Newtonsoft 需要）
//
// 图片：放在 Assets/StreamingAssets/PropImages/ 下，imagePath 只存文件名（含扩展名）
//      运行时调用 PropImageLoader.Load(imagePath) 拿 Sprite

using Taffy.Data;
using Taffy.OverAllManager;
using static UnityEngine.Rendering.DebugUI;
using Unity.VisualScripting;
using UnityEngine.Rendering;

namespace Taffy.Data
{
    /// <summary>
    /// 道具都有归属于的玩家，看看是归属于谁
    /// </summary>
    public enum PropOwner { A, B, Public }

    // ── 基类 ─────────────────────────────────────────────────────────────────

    public abstract class Prop
    {
        public string name = "NullProp";
        public string description = "无";
        public string imagePath = "NullProp.png";
        public int value = 0;
        public int playingQuantity = -1;     //-1意味着游戏中数值没用
        public float maxAttackDistance = 2;
        public PropRarity rarity = PropRarity.普通;
        public PropOwner owner = PropOwner.Public;
    }

    // ── 接口（playingQuantity 的语义） ────────────────────────────────────────

    public interface IWeapon
    {
        int ATK { get; }

        public void AssignATK(PropOwner user)
        {
            if (user == PropOwner.A) OverAllPlayerController.Instance.AssignATK_A(ATK);
            else if (user == PropOwner.B) OverAllPlayerController.Instance.AssignATK_B(ATK);
        }
    }

    public interface IDefend
    {
        int DEF { get; }

        public void AssignDEF(PropOwner owner)
        {
            if (owner == PropOwner.A) OverAllPlayerController.Instance.AssignDEF_A(DEF);
            else if (owner == PropOwner.B) OverAllPlayerController.Instance.AssignDEF_B(DEF);
        }
    }
    public interface IRemoteAttack
    {
        public void LaunchObject();
    }
    public interface ITreasure { }

    public interface ICure { int Curative { get; } }

    public interface ICultivate { void BonusEffect(PropOwner beneficiary); }
    // ── 默认类 ───────────────────────────────────────────────────────────────

    /// <summary>
    /// 剑
    /// </summary>
    public class Sword : Prop, IWeapon
    {
        public int ATK => playingQuantity;

        public Sword()
        {
            name = "剑";
            description = "普通的剑";
            imagePath = "Sword.png";
            value = 50;
            playingQuantity = 10;
            rarity = PropRarity.普通;
            owner = PropOwner.B;
        }
    }

    /// <summary>
    /// 护甲
    /// </summary>
    public class Armor : Prop, IDefend
    {
        public int DEF => playingQuantity;

        public Armor()
        {
            name = "防具皮";
            description = "普通的护甲";
            imagePath = "Armor.png";
            value = 50;
            playingQuantity = 5;
            rarity = PropRarity.普通;
        }
    }

    /// <summary>
    /// 金币，纯收藏宝物都可以继承它
    /// </summary>
    public class Coin : Prop, ITreasure
    {
        public Coin()
        {
            name = "金币";
            description = "意味着最小面值";
            imagePath = "Coin.png";
            value = 1;
            rarity = PropRarity.普通;
        }
    }

    /// <summary>
    /// 弓
    /// </summary>
    public class Bow : Prop, IWeapon, IRemoteAttack
    {
        public int ATK => playingQuantity;

        public Bow()
        {
            name = "弓";
            description = "普通的弓";
            imagePath = "Bow.png";
            value = 60;
            playingQuantity = 8;
            maxAttackDistance = 20;
            rarity = PropRarity.普通;
            owner = PropOwner.A;
        }

        public void LaunchObject()
        {

        }
    }

    public class CurePotion : Prop, ICure
    {
        public int Curative => playingQuantity;
        public CurePotion()
        {
            name = "回复药水";
            description = "能回血";
            imagePath = "CurePotion.png";
            value = 80;
            playingQuantity = 20;
            rarity = PropRarity.普通;
        }
    }

    public class HeartFruit : Prop, ICultivate
    {
        public HeartFruit()
        {
            name = "心形果";
            description = "吃了可以增加血量上限";
            imagePath = "HeartFruit.png";
            value = 1500;
            playingQuantity = 10;
            rarity = PropRarity.稀有;
        }

        public void BonusEffect(PropOwner beneficiary)
        {
            if (beneficiary == PropOwner.A) OverAllPlayerController.Instance.AddMaxHP_A(playingQuantity);
            else if (beneficiary == PropOwner.B) OverAllPlayerController.Instance.AddMaxHP_B(playingQuantity);
        }
    }

    // ── 具体类：只改数值的子类，构造函数里覆盖父类默认值 ─────────────────────

    public class BigSword : Sword
    {
        public BigSword()
        {
            name = "大剑";
            description = "更大的剑";
            imagePath = "Big_Sword.png";
            value = 100;
            playingQuantity = 100;
            rarity = PropRarity.稀有;
        }
    }
    /// <summary>
    /// 大弓
    /// </summary>
    public class BigBow : Bow
    {
        public BigBow()
        {
            name = "大弓";
            description = "更大的弓";
            imagePath = "Big_Bow.png";
            value = 120;
            playingQuantity = 18;
            maxAttackDistance = 30;
            rarity = PropRarity.稀有;
        }
    }
    public class BigestBow : Bow
    {
        public BigestBow()
        {
            name = "特大弓";
            description = "最大的弓";
            imagePath = "Bigest_Bow.png";
            value = 220;
            playingQuantity = 30;
            maxAttackDistance = 40;
            rarity = PropRarity.传说;
        }
    }

    public class TaffyPhoto : Prop, ITreasure
    {
        public TaffyPhoto()
        {
            name = "塔菲美照";
            description = "塔菲绝版照片";
            imagePath = "TaffyPhoto.png";
            value = 3100;
            rarity = PropRarity.传说;
        }
    }

    public class ALotOfCoins : Prop, ITreasure
    {
        public ALotOfCoins()
        {
            name = "一堆金币";
            description = "大概五百个";
            imagePath = "ALotOfCoins.png";
            value = 500;
            rarity = PropRarity.稀有;
        }
    }
    public class BigestSword : Sword
    {
        public BigestSword()
        {
            name = "特大剑";
            description = "最大的剑";
            imagePath = "Bigest_Sword.png";
            value = 200;
            playingQuantity = 200;
            rarity = PropRarity.传说;
        }
        
    }
    /// <summary>
    /// 塔菲手办
    /// </summary>
    public class TaffyFigure : Prop, ITreasure
    {
        public TaffyFigure()
        {
            name = "塔菲手办";
            description = "精致到离谱的塔菲手办,关注塔菲谢谢喵";
            imagePath = "TaffyFigure.png";
            value = 2000;
            rarity = PropRarity.传说;
        }

    }
    /// <summary>
    /// 建材
    /// </summary>
    public class Match : Prop, ITreasure
    {
        public Match()
        {
            name = "火柴";
            description = "";
            imagePath = "Match.png";
            value = 200;
            rarity = PropRarity.普通;
        }
    }
    public class Pipe : Prop, ITreasure
    {
        public Pipe()
        {
            name = "管子";
            description = "";
            imagePath = "Pipe.png";
            value = 300;
            rarity = PropRarity.普通;
        }
    }
    
    public class Electric_Drille : Prop, ITreasure
    {
        public Electric_Drille()
        {
            name = "电钻";
            description = "";
            imagePath = "Electric——Drill.png";
            value = 800;
            rarity = PropRarity.稀有;
        }
    }
    public class TNT : Prop, ITreasure
    {
        public TNT()
        {
            name = "电钻";
            description = "";
            imagePath = "TNT.png";
            value = 1000;
            rarity = PropRarity.稀有;
        }
    }
    public class Handsaw : Prop, ITreasure
    {
        public Handsaw()
        {
            name = "手锯";
            description = "";
            imagePath = "Handsaw.png";
            value = 1200;
            rarity = PropRarity.稀有;
        }
    }
    public class Hardwood : Prop, ITreasure
    {
        public Hardwood()
        {
            name = "硬木";
            description = "";
            imagePath = "Hardwood.png";
            value = 400;
            rarity = PropRarity.普通;
        }
    }
    /// <summary>
    /// 柔软的东西
    /// </summary>
    public class Tarp : Prop, ITreasure
    {
        public Tarp()
        {
            name = "防水布";
            description = "";
            imagePath = "Tarp.png";
            value = 600;
            rarity = PropRarity.普通;
        }
    }
    public class Fast_Pillow : Prop, ITreasure
    {
        public Fast_Pillow()
        {
            name = "姬野星奏抱枕";
            description = "奏了奏了";
            imagePath = "Fast_Pillow.png";
            value = 1000;
            rarity = PropRarity.稀有;
        }
    }
    public class Baseball_Pillow : Prop, ITreasure
    {
        public Baseball_Pillow()
        {
            name = "曾根美雪抱枕";
            description = "你出轨了是吧？";
            imagePath = "Baseball_Pillow.png";
            value = 1000;
            rarity = PropRarity.稀有;
        }
    }
    public class Glitter_Ball : Prop, ITreasure
    {
        public Glitter_Ball()
        {
            name = "炫彩精灵球";
            description = "用于捕捉雪影娃娃";
            imagePath = "Glitter_Ball.png";
            value = 1800;
            rarity = PropRarity.传说;
        }
    }


}


