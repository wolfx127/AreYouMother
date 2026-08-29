using System.Collections.Generic;
using Taffy.Data;
using Taffy.Data.PropData;
using UnityEngine;

namespace Taffy.Play.Container
{
    public static class ContainerCreatorTool
    {
        private static List<Prop> Common_Common = new List<Prop>();
        private static List<Prop> Cure_Common = new List<Prop>();
        private static List<Prop> Weapon_Common  = new List<Prop>();
        private static List<Prop> Armor_Common  = new List<Prop>();
        private static List<Prop> Treasure_Common = new List<Prop>();
        
        private static List<Prop> Common_Unusual = new List<Prop>();
        private static List<Prop> Cure_Unusual = new List<Prop>();
        private static List<Prop> Weapon_Unusual  = new List<Prop>();
        private static List<Prop> Armor_Unusual  = new List<Prop>();
        private static List<Prop> Treasure_Unusual = new List<Prop>();
        
        private static List<Prop> Common_Rare = new List<Prop>();
        private static List<Prop> Cure_Rare = new List<Prop>();
        private static List<Prop> Weapon_Rare =  new List<Prop>();
        private static List<Prop> Armor_Rare =  new List<Prop>();
        private static List<Prop> Treasure_Rare =  new List<Prop>();
        
        private static List<Prop> Common_Legend = new List<Prop>();
        private static List<Prop> Cure_Legend = new List<Prop>();
        private static List<Prop> Weapon_Legend =  new List<Prop>();
        private static List<Prop> Armor_Legend =  new List<Prop>();
        private static List<Prop> Treasure_Legend =  new List<Prop>();

        
        public static void Build()
        {
            Debug.Log("[初始化] ContainerCreatorTool 开始分桶");

            Common_Common.Clear();   Common_Unusual.Clear();   Common_Rare.Clear();   Common_Legend.Clear();
            Cure_Common.Clear();     Cure_Unusual.Clear();     Cure_Rare.Clear();     Cure_Legend.Clear();
            Weapon_Common.Clear();   Weapon_Unusual.Clear();   Weapon_Rare.Clear();   Weapon_Legend.Clear();
            Armor_Common.Clear();    Armor_Unusual.Clear();    Armor_Rare.Clear();    Armor_Legend.Clear();
            Treasure_Common.Clear(); Treasure_Unusual.Clear(); Treasure_Rare.Clear(); Treasure_Legend.Clear();

            foreach (var so in PropList.propSOList)
            {
                var p = new Prop(so);

                switch (so.containerType)
                {
                    case ContainerType.Common:   AddByRarity(p, Common_Common,   Common_Unusual,   Common_Rare,   Common_Legend);   break;
                    case ContainerType.Cure:     AddByRarity(p, Cure_Common,     Cure_Unusual,     Cure_Rare,     Cure_Legend);     break;
                    case ContainerType.Weapon:   AddByRarity(p, Weapon_Common,   Weapon_Unusual,   Weapon_Rare,   Weapon_Legend);   break;
                    case ContainerType.Armor:    AddByRarity(p, Armor_Common,    Armor_Unusual,    Armor_Rare,    Armor_Legend);    break;
                    case ContainerType.Treasure: AddByRarity(p, Treasure_Common, Treasure_Unusual, Treasure_Rare, Treasure_Legend); break;
                }
            }

            int total = Common_Common.Count + Common_Unusual.Count + Common_Rare.Count + Common_Legend.Count
                      + Cure_Common.Count + Cure_Unusual.Count + Cure_Rare.Count + Cure_Legend.Count
                      + Weapon_Common.Count + Weapon_Unusual.Count + Weapon_Rare.Count + Weapon_Legend.Count
                      + Armor_Common.Count + Armor_Unusual.Count + Armor_Rare.Count + Armor_Legend.Count
                      + Treasure_Common.Count + Treasure_Unusual.Count + Treasure_Rare.Count + Treasure_Legend.Count;
            Debug.Log($"[初始化] ContainerCreatorTool 分桶完成，共 {total} 个Prop");
        }

        private static void AddByRarity(Prop p, List<Prop> common, List<Prop> unusual, List<Prop> rare, List<Prop> legend)
        {
            switch (p.rarity)
            {
                case Rarity.Common:   common.Add(p);  break;
                case Rarity.Unusual:  unusual.Add(p); break;
                case Rarity.Rare:     rare.Add(p);    break;
                case Rarity.Legend:   legend.Add(p);  break;
            }
        }

        public static List<Prop> GetUnionList(ContainerType containerType, Rarity rarity)
        {
            switch (containerType)
            {
                case ContainerType.Common:
                    switch (rarity)
                    {
                        case Rarity.Common:  return Common_Common;
                        case Rarity.Unusual: return Common_Unusual;
                        case Rarity.Rare:    return Common_Rare;
                        case Rarity.Legend:  return Common_Legend;
                    }
                    break;
                case ContainerType.Cure:
                    switch (rarity)
                    {
                        case Rarity.Common:  return Cure_Common;
                        case Rarity.Unusual: return Cure_Unusual;
                        case Rarity.Rare:    return Cure_Rare;
                        case Rarity.Legend:  return Cure_Legend;
                    }
                    break;
                case ContainerType.Weapon:
                    switch (rarity)
                    {
                        case Rarity.Common:  return Weapon_Common;
                        case Rarity.Unusual: return Weapon_Unusual;
                        case Rarity.Rare:    return Weapon_Rare;
                        case Rarity.Legend:  return Weapon_Legend;
                    }
                    break;
                case ContainerType.Armor:
                    switch (rarity)
                    {
                        case Rarity.Common:  return Armor_Common;
                        case Rarity.Unusual: return Armor_Unusual;
                        case Rarity.Rare:    return Armor_Rare;
                        case Rarity.Legend:  return Armor_Legend;
                    }
                    break;
                case ContainerType.Treasure:
                    switch (rarity)
                    {
                        case Rarity.Common:  return Treasure_Common;
                        case Rarity.Unusual: return Treasure_Unusual;
                        case Rarity.Rare:    return Treasure_Rare;
                        case Rarity.Legend:  return Treasure_Legend;
                    }
                    break;
            }
            return null;
        }
    }
}
