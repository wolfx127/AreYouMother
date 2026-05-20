using UnityEngine;

//如何使用道具Prop？
//1.先拿到具体类的实例
////1.1.这里的具体类说的就是继承了Prop的类，比如class Sword : Prop。
////1.2.实例化需要：创建一个PropSO文件，脚本里拿到这个SO的引用，然后把这个SO引用填进构造函数里。
////   比如public PropSO BigSword;
////   //在unity编辑器里把创建好的SO文件拖进BigSword字段。
////   Sword sword = new Sword(BigSword);
//////1.2.1.如何创建PropSO文件？unity编辑器的Project窗口内，找到"Assets/Scripts/Taffy/Data/PropVariety_SO"右键选项Create->Prop->PropVariety
////1.3.实例化怎么填数值？同时可以回答为什么实例化需要SO。为了写数值需要创建SO，Project窗口内点击这个SO，再在Inspector窗口填数值。构造函数会读SO，会把这些数值填进具体类的实例里
//
//2.调用数值playingQuantity
////2.1.数值playingQuantity是和继承的接口相关的。比如一个能攻击敌人的剑Sword，它就继承了 能攻击接口Iweapon ，因为继承了Iweapon，所以Sword的playingQuantity就变成攻击力了
////2.2.怎么知道数值playingQuantity是干什么用的？看它类型是不是接口类型，比如已经有一个实例sword了，那就执行sword is Iweapon;true就执行 int攻击力 = sword.playingQuantity;
//
//3.怎么自定义道具？
////3.1.声明具体类继承Prop
////3.2.声明接口并实现，用于自定义数值playingQuantity的用法,也可以在接口里写其他行为。
////3.3.SO可以自定义数值
////3.4.接口里可以什么都不写，但是这种接口不能一次性继承两个。里面写了东西的接口不能管数值playingQuantity。这些都是代码逻辑合理，但是不能保证规范，调用起来会越来越麻烦。
//
//4.如何制造一个防御力120的铁头盔IronHelmet实例？
////4.1.首先声明盔甲类继承Prop，就叫Armor吧，所有不同种类的盔甲都来自该类，不管是皮胸甲还是铁头盔
////    public class Armor : Prop {}
////4.2.让Armor的数值能当做防御力用，就得声明一个防御接口，然后继承接口。接口就起名叫IDefend吧
////    public interface IDefend { }
////    public class Armor: Prop, IDefend {}
////4.3.实现抽象类Prop
////    public class Armor: Prop, IDefend
////   {
////        public Armor(PropSO propSO):base(propSO){ }
////   }
////4.4.退到unity编辑器，创建一个PropSO，起名叫IronHelmetSO，在inspector窗口playingQuantity那里填数值120
////4.5.另找一个脚本(需要实例化一个铁头盔的脚本)，获取IronHelmetSO的引用，那么就需要声明引用类型变量接住。
////    public PropSO ironHelmetSO;//编辑器内把IronHelmetSO拖到这个字段里。注意，能拖进来得需要改脚本继承MonoBehavior
////4.6.实例化一个IronHelmet出来
////    public IronHelmet = new Armor(ironHelmetSO);


namespace Taffy.Data
{
    public enum PropOwner { A, B, Public }

    //abstract 定义 道具 这个大类
    public abstract class Prop
    {
        public int value = 0;//买卖价格
        public int playingQuantity = 0;//数值
        public string name = "ThisIsNullProp";//名字
        public PropOwner owner = PropOwner.Public;
        /// <summary>
        /// 该实例名字讲取决于SO的名字，后续改名直接找SO按f12重命名
        /// </summary>
        /// <param name="propSo"></param>
        protected Prop(PropSO propSo)
        {
            value = propSo.value;
            playingQuantity = propSo.playingQuantity;
            name = propSo.name.EndsWith("SO") ? propSo.name[..^2] : propSo.name;
        }
    }
    
    

    //interface 定义 行为 。主要是定义playingQuantity。比如是武器的类，继承了IWeapon，那playingQuantity就变成攻击力了
    //想加方法不在这加，在下面具体的类里加
    public interface IWeapon { }
    public interface ITreasure { }
    
    

    //concrete class 具体的类。既是道具，又能组合各种行为。比如剑就是继承了Prop和IWeapon
    //附加方法在这里加
    //附加变量看情况在这里加吧，反正也没很多加变量的时候了

    public class Sword : Prop, IWeapon
    {
        public Sword(PropSO propSO) : base(propSO) { }
    }

    public class Cion : Prop, ITreasure
    {
        public Cion(PropSO propSo) : base(propSo) { }
    }



    //SO 负责填各种不同道具的数值。怎么填？在unity创建so文件，然后在inspector里填数值。那着这个创建好的so文件，在实例化时填进构造函数里
    [CreateAssetMenu(fileName = "NewPropVariety", menuName = "Prop/PropVariety")]
    public class PropSO : ScriptableObject
    {
        public PropOwner owner = PropOwner.Public;
        public int value = 0;
        public int playingQuantity = 0;
    }
}
