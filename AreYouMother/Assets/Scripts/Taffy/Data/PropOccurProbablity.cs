using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace Taffy.Data
{
    public enum ContainerType { Common,Cure,Weapon,Armor,Treasure }
    public enum PropRarity { Common, Rare, Legend }

    public static class PropOccurProbability
    {
        
        public static Type[] CommonProps { get; private set; }
        public static Type[] RareProps   { get; private set; }
        public static Type[] LegendProps { get; private set; }

        public static void Build()
        {
            var common = new List<Type>();
            var rare = new List<Type>();
            var legend = new List<Type>();

            var baseType = typeof(Prop);
            foreach (var t in Assembly.GetAssembly(baseType).GetTypes())
            {
                if (t.IsAbstract || !t.IsSubclassOf(baseType)) continue;
                var instance = (Prop)Activator.CreateInstance(t);
                switch (instance.rarity)
                {
                    case PropRarity.Common:
                        common.Add(t);
                        Debug.Log($"普通稀有度加进一种道具 {t}");
                        break;
                    case PropRarity.Rare:
                        rare.Add(t);
                        Debug.Log($"稀有稀有度加进一种道具 {t}");
                        break;
                    case PropRarity.Legend:
                        legend.Add(t);
                        Debug.Log($"传奇稀有度加进一种道具 {t}");
                        break;
                }
            }

            CommonProps = common.ToArray();
            RareProps = rare.ToArray();
            LegendProps = legend.ToArray();
        }
    }
    
    public static class PropOccurType
    {
        public static readonly Type[] CommonProp = { typeof(Coin), typeof(Sword) };
        public static readonly Type[] CureProp = { typeof(CurePotion) ,typeof(HeartFruit)};
        public static readonly Type[] WeaponProp = { typeof(Sword),typeof(BigSword) ,typeof(Bow),typeof(BigBow)};
        public static readonly Type[] ArmorProp = { typeof(Armor)};
        public static readonly Type[] TreasureProp = { typeof(TaffyPhoto)};
    }
}
