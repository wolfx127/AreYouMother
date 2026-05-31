
// 道具使用方式
// 1. 中间类（Sword/Armor/Coin/...）规定一类道具共享的字段与接口
// 2. 具体类继承中间类，在无参构造函数里写默认数值；BigSword : Sword 这种"只改数值"的子类只需重写要改的字段
// 3. 实例化：new BigSword()，序列化/反序列化都靠无参构造（Newtonsoft 需要）
//
// 图片：放在 Assets/StreamingAssets/PropImages/ 下，imagePath 只存文件名（含扩展名）
//      运行时调用 PropImageLoader.Load(imagePath) 拿 Sprite

using Taffy.Home;
using Taffy.OverAllManager;
using Taffy.Play.Player;
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
        public string     name             = "NullProp";
        public string     description      = "无";
        public string     imagePath        = "NullProp.png";
        public int        value            = 0;
        public int        playingQuantity  = -1;     //-1意味着游戏中数值没用
        public float      maxAttackDistance   = 2;
        public int costMP = 0;
        public PropRarity rarity           = PropRarity.Common;
        public PropOwner  owner            = PropOwner.Public;
    }

    // ── 接口（playingQuantity 的语义） ────────────────────────────────────────

    public interface IWeapon
    {
        int ATK  { get; }

        public void AssignATK(PropOwner user)
        {
            if(user == PropOwner.A) OverAllPlayerController.Instance.AssignATK_A(ATK);
            else if(user == PropOwner.B) OverAllPlayerController.Instance.AssignATK_B(ATK);
        }
    }

    public interface IDefend
    {
        int DEF { get; }

        public void AssignDEF(PropOwner owner)
        {
            if(owner == PropOwner.A) OverAllPlayerController.Instance.AssignDEF_A(DEF);
            else if(owner == PropOwner.B) OverAllPlayerController.Instance.AssignDEF_B(DEF);
        }
    }
    public interface IRemoteAttack
    {
        public void LaunchObject();
    }
    public interface ITreasure { }

    public interface ICure { int Curative { get;  } }

    public interface ICultivate { void BonusEffect(PropOwner beneficiary); }

    public interface IUsable
    {
        void UseEffect(PropOwner beneficiary);
    }
    // ── 默认类 ───────────────────────────────────────────────────────────────

    /// <summary>
    /// 剑
    /// </summary>
    public class Sword : Prop, IWeapon
    {
        public int ATK => playingQuantity;

        public Sword()
        {
            name             = "剑";
            description      = "普通的剑";
            imagePath        = "Sword.png";
            value            = 50;
            playingQuantity  = 10;
            rarity = PropRarity.Common;
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
            name             = "防具皮";
            description      = "普通的护甲";
            imagePath        = "Armor.png";
            value            = 50;
            playingQuantity  = 5;
            rarity = PropRarity.Common;
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
            imagePath   = "Coin.png";
            value       = 1;
            rarity = PropRarity.Common;
        }
    }
    
    /// <summary>
    /// 弓
    /// </summary>
    public class Bow : Prop, IWeapon , IRemoteAttack
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
            rarity = PropRarity.Common;
            owner = PropOwner.A;
        }

        public void LaunchObject()
        {
            
        }
    }
    
    public class CurePotion : Prop,ICure,IUsable
    {
        public int Curative => playingQuantity;
        public CurePotion()
        {
            name        = "回复药水";
            description = "能回血";
            imagePath = "CurePotion.png";
            value = 80;
            playingQuantity = 10;
            rarity = PropRarity.Common;
        }

        public void UseEffect(PropOwner beneficiary)
        {
            if(beneficiary is PropOwner.A) PlayerCurrentStateController.Instance.Cure_A(playingQuantity);
            if(beneficiary is PropOwner.B) PlayerCurrentStateController.Instance.Cure_B(playingQuantity);
        }
    }

    public class HeartFruit:Prop, ICultivate
    {
        public HeartFruit()
        {
            name = "心形果";
            description = "吃了可以增加血量上限";
            imagePath = "HeartFruit.png";
            value = 1500;
            playingQuantity = 10;
            rarity = PropRarity.Rare;
        }

        public void BonusEffect(PropOwner beneficiary)
        {
            if(beneficiary == PropOwner.A) OverAllPlayerController.Instance.AddMaxHP_A(playingQuantity);
            else if (beneficiary == PropOwner.B) OverAllPlayerController.Instance.AddMaxHP_B(playingQuantity);
        }
    }

    public class GiftBox : Prop, ICultivate
    {
        public GiftBox()
        {
            name = "礼品盒";
            description = "能加商人好感度。盒子里有什么不知道";
            imagePath = "GiftBox.png";
            value = 100;
            playingQuantity = 3;
            rarity = PropRarity.Common;
        }

        /// <summary>
        /// 随便填参数，谁都能用
        /// </summary>
        /// <param name="beneficiary"></param>
        public void BonusEffect(PropOwner beneficiary)
        {
            DealerManager.AddFavoribility(playingQuantity);
        }
    }

    // ── 具体类：只改数值的子类，构造函数里覆盖父类默认值 ─────────────────────

    public class BigSword : Sword
    {
        public BigSword()
        {
            name        = "大剑";
            description = "更大的剑";
            imagePath   = "Big_Sword.png";
            value       = 100;
            playingQuantity = 100;
            rarity = PropRarity.Rare;
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
            rarity = PropRarity.Rare;
        }
    }


    public class TaffyPhoto : Prop, ITreasure
    {
        public TaffyPhoto()
        {
            name        = "塔菲美照";
            description = "塔菲绝版照片";
            imagePath = "TaffyPhoto.png";
            value = 3100;
            rarity = PropRarity.Legend;
        }
    }

    public class ALotOfCoins : Prop, ITreasure
    {
        public ALotOfCoins()
        {
            name        = "一堆金币";
            description = "大概五百个";
            imagePath = "ALotOfCoins.png";
            value = 500;
            rarity = PropRarity.Rare;
        }
    }
    public class FastPillow : Prop, ITreasure
    {
        public FastPillow()
        {
            name = "姬野星奏抱枕";
            description = "奏了奏了";
            imagePath = "FastPillow.png";
            value = 2000;
            rarity = PropRarity.Rare;
        }
    }
    public class BaseballPillow : Prop, ITreasure
    {
        public BaseballPillow()
        {
            name = "曾根美雪抱枕";
            description = "别出轨";
            imagePath = "BaseballPillow.png";
            value = 2100;
            rarity = PropRarity.Rare;
        }
    }
    public class GlitterBall : Prop, ITreasure
    {
        public GlitterBall()
        {
            name = "炫彩球";
            description = "用于捕捉雪影娃娃";
            imagePath = "GlitterBall.png";
            value = 3000;
            rarity = PropRarity.Legend;
        }
    }
    public class Match : Prop, ITreasure
    {
        public Match()
        {
            name = "火柴";
            description = "可以生火";
            imagePath = "Match.png";
            value = 300;
            rarity = PropRarity.Common;
        }
    }
    public class Pipe : Prop, ITreasure
    {
        public Pipe()
        {
            name = "管子";
            description = "";
            imagePath = "Pipe.png";
            value = 500;
            rarity = PropRarity.Common;
        }
    }
    public class Handsaw : Prop, ITreasure
    {
        public Handsaw()
        {
            name = "手锯";
            description = "锯木必备";
            imagePath = "Handsaw.png";
            value = 800;
            rarity = PropRarity.Rare;
        }
    }
    public class Hardwood : Prop, ITreasure
    {
        public Hardwood()
        {
            name = "硬木";
            description = "极佳的木材";
            imagePath = "Hardwood.png";
            value = 600;
            rarity = PropRarity.Common;
        }
    }
    public class Tarp : Prop, ITreasure
    {
        public Tarp()
        {
            name = "防水布";
            description = "遮风挡雨";
            imagePath = "Tarp.png";
            value = 1000;
            rarity = PropRarity.Rare;
        }
    }
    

}
