using UnityEngine;

namespace Taffy.Data
{
    //abstract 定义 道具 这个大类
    public abstract class Prop
    {
        public int value = 0;
        public int playingQuantity = 0;
        public string name = "";
        public Prop(PropSO propSo)
        {
            value = propSo.value;
            playingQuantity = propSo.playingQuantity;
            name = propSo.name;
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
        public int value = 0;
        public int playingQuantity = 0;
    }
}
