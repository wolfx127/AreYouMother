using System;
using System.Collections.Generic;
using System.Linq;
using Taffy.OverAllManager;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace Taffy.Data.PropData
{
    public enum PropType
    {
        [InspectorName("默认")]Default,
        [InspectorName("[Home]一次消耗性")] Cultivate,
        [InspectorName("[Play]一次消耗性")] Consume,
        [InspectorName("收藏品")]Treasure,
        [InspectorName("可近战")]Close_Attack,
        [InspectorName("可远攻")]Remote_Attack,
        [InspectorName("可防御")]Defend,
        [InspectorName("[Play]可治疗")]AddBlood,
        [InspectorName("[Play]可回蓝")]AddSkill,
        [InspectorName("[Home]可提升血量上限")]AddMaxBlood,
        [InspectorName("[Home]可提升蓝条上限")]AddMaxSkill,
        [InspectorName("[Home]可提升商人好感度")]AddFavorability,
        [InspectorName("[Play]攻击使敌人中毒一段时间")]Poison,
        [InspectorName("[Play]解自身负面效果的剩余时间")]Detoxify,
    }
    
    public enum ContainerType
    {
        [InspectorName("普通箱")]Common,
        [InspectorName("医疗箱")]Cure,
        [InspectorName("武器箱")]Weapon,
        [InspectorName("防具箱")]Armor,
        [InspectorName("保险箱")]Treasure
    }

    public enum Rarity
    {
        [InspectorName("普通")]Common,
        [InspectorName("稀有")]Rare,
        [InspectorName("罕见")]Unusual,
        [InspectorName("传说")]Legend,
    }

    public enum PropOwner
    {
        A,
        B,
        Public
    }

    [Serializable]
    public struct PropBehavior_Value
    {
        public PropType type;
        public int value;
    }

    public class Prop
    {
        public string name;
        public PropOwner owner;
        public PropBehavior_Value[] behavior_value;
        public ContainerType containerType;
        public string description;
        public int price = 0;
        public Rarity rarity = Rarity.Common;
        public Texture2D image;

        public Prop(string name = " ",
            PropOwner owner = PropOwner.Public,
            PropBehavior_Value[] behavior_value = null,
            ContainerType containerType = ContainerType.Common,
            string description = " ",
            int price = 0,
            Rarity rarity = Rarity.Common,
            Texture2D image = null)
        {
            this.name = name;
            this.owner = owner;
            this.behavior_value = behavior_value;
            this.containerType = containerType;
            this.description = description;
            this.price = price;
            this.rarity = rarity;
            this.image = image;
        }

        public Prop(PropSO propSO)
        {
            name = propSO.name;
            owner = propSO.owner;
            behavior_value = (PropBehavior_Value[])propSO.behavior_value.Clone();
            containerType = propSO.containerType;
            description = propSO.description;
            price = propSO.price;
            rarity = propSO.rarity;
            image = propSO.image;
        }

        public Prop Clone()
        {
            return new Prop(name, owner, behavior_value, containerType, description, price, rarity, image);
        }

        public bool IsEqual(PropSO propSO)
        {
            if(propSO.name != name) return false;
            if(propSO.owner != owner) return false;
            if(!propSO.behavior_value.SequenceEqual(behavior_value)) return false;
            if(propSO.price != price) return false;
            return true;
        }

        public PropJson ToJson()
        {
            return new PropJson(name, owner, behavior_value, containerType, price, rarity);
        }

        /// <summary>
        /// 返回值表示使用后是否删除
        /// </summary>
        public bool Execute(Prop prop = null, char Player = ' ')
        {
            bool shouldDelete = false;
            foreach (var behavior in behavior_value)
            {
                PropBehaviorTable.table[behavior.type].Execute(prop,Player,behavior.value);
                if(behavior.type == PropType.Cultivate && OverAllStates.isInHome) shouldDelete = true;
                else if (behavior.type == PropType.Consume && OverAllStates.isInPlay) shouldDelete = true;
            }
            return shouldDelete;
        }
    }

    public static class PropList
    {
        public static List<PropSO> propSOList = new List<PropSO>();
        public static List<Prop> propList = new List<Prop>();
        public static Dictionary<PropJson, Prop> propJsonTable = new Dictionary<PropJson, Prop>();

        public static void BuildList()
        {
            var handle = Addressables.LoadAssetsAsync<PropSO>("PropSO");//对包发送请求，返回一个订单。实际上叫句柄，句柄不持有资源，但是有个类似指针的东西指向资源，还可以通过句柄查加载进度，但是句柄不持有加载进度。类比订单号和查询订单进度
            handle.WaitForCompletion();//保证上面那行异步执行完毕，再执行下一行。底层是一直执行不返回，检查句柄，如果没加载完就一直执行，因为它不返回所以才一直卡着主线程
            propSOList.Clear();
            propList.Clear();
            propJsonTable.Clear();
            propSOList.AddRange(handle.Result);//把订单里记的资源全塞进list里
            Debug.Log($"[初始化] PropList 加载完成，共 {propSOList.Count} 个PropSO");

            foreach (var propSO in propSOList)
            {
                propList.Add(new Prop(propSO));
            }

            foreach (var prop in propList)
            {
                propJsonTable[prop.ToJson()] = prop;
            }
        }
    }

    public class PropJson
    {
        public string name;
        public PropOwner owner;
        public PropBehavior_Value[] behavior_value;
        public ContainerType containerType;
        public int price = 0;
        public Rarity rarity = Rarity.Common;

        public PropJson(string name,PropOwner owner,PropBehavior_Value[] behavior_value,ContainerType containerType,int price,Rarity rarity)
        {
            this.name = name;
            this.owner = owner;
            this.behavior_value = behavior_value;
            this.containerType = containerType;
            this.price = price;
            this.rarity = rarity;
        }

        public override bool Equals(object obj)
        {
            var other = obj as PropJson;
            if (other == null) return false;
            if (name != other.name) return false;
            if (owner != other.owner) return false;
            if (containerType != other.containerType) return false;
            if (price != other.price) return false;
            if (rarity != other.rarity) return false;

            if (behavior_value == null || other.behavior_value == null)
                return behavior_value == other.behavior_value;
            return behavior_value.SequenceEqual(other.behavior_value);
        }

        public override int GetHashCode()
        {
            int hash = 17;
            hash = hash * 31 + (name == null ? 0 : name.GetHashCode());
            hash = hash * 31 + (int)owner;
            hash = hash * 31 + (int)containerType;
            hash = hash * 31 + price;
            hash = hash * 31 + (int)rarity;
            if (behavior_value != null)
                foreach (var b in behavior_value)
                    hash = hash * 31 + b.GetHashCode();
            return hash;
        }

        public Prop DeJson()
        {
            if (PropList.propJsonTable.Count == 0)
            {
                PropList.BuildList();   // Awake 顺序问题：读档跑在注册表构建之前，这里兜底建一次
                if (PropList.propJsonTable.Count == 0)
                {
                    Debug.Log("没法反序列化道具，道具注册表没加载完");
                    return null;
                }
            }

            PropList.propJsonTable.TryGetValue(this, out var prop);
            return prop;
        }
    }
}