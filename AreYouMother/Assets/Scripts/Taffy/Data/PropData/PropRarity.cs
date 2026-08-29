using System.Collections.Generic;
using UnityEngine;

namespace Taffy.Data.PropData
{
    public static class PropRarity
    {
        public static List<PropData.Prop> Common =  new List<PropData.Prop>();
        public static List<PropData.Prop> Unusual = new List<PropData.Prop>();
        public static List<PropData.Prop> Rare = new List<PropData.Prop>();
        public static List<PropData.Prop> Legend = new List<PropData.Prop>();

        
        public static void Build()
        {
            Common.Clear();
            Unusual.Clear();
            Rare.Clear();
            Legend.Clear();

            foreach (var propSO in PropList.propSOList)
            {
                if(propSO.rarity == Rarity.Common) Common.Add(new  PropData.Prop(propSO));
                if(propSO.rarity == Rarity.Unusual) Unusual.Add(new  PropData.Prop(propSO));
                if(propSO.rarity == Rarity.Rare) Rare.Add(new  PropData.Prop(propSO));
                if(propSO.rarity == Rarity.Legend) Legend.Add(new  PropData.Prop(propSO));
            }
        }
        
        //从100个数里划分出四段，段长度越小爆率越低。字段值表示段上限
        private static int common = 50;
        private static int unusual = 70;
        private static int rare = 90;
        private static int legend = 100;
        
        public static PropData.Prop GetRandomPropByRarity()
        {
            int probability = Random.Range(0, 101);
            if (probability < common) return PickOne(Common);
            else if (probability < unusual) return PickOne(Unusual);
            else if (probability < rare) return PickOne(Rare);
            else return PickOne(Legend);

            static PropData.Prop PickOne(List<PropData.Prop> pool)
                => pool.Count == 0 ? null : pool[Random.Range(0, pool.Count)].Clone();
        }

        /// <summary>按 IsEqual 在四个稀有度桶里找与某个 SO 对应的 Prop，找不到返回 null</summary>
        public static PropData.Prop Find(PropSO so)
        {
            foreach (var p in Common)   if (p.IsEqual(so)) return p;
            foreach (var p in Unusual)  if (p.IsEqual(so)) return p;
            foreach (var p in Rare)     if (p.IsEqual(so)) return p;
            foreach (var p in Legend)   if (p.IsEqual(so)) return p;
            return null;
        }

        public static Rarity GetRandomRarity()
        {
            int probability = Random.Range(0, 101);
            if (probability < common) return Rarity.Common;
            else if (probability < unusual) return Rarity.Unusual;
            else if (probability < rare) return Rarity.Rare;
            else return Rarity.Legend;
        }
    }
}
