using System;
using System.Collections.Generic;
using Taffy.Data;
using UnityEngine;

namespace Taffy.Home
{
    public static class DealerManager
    {
        /// <summary>
        /// 只能保存数据时调用
        /// </summary>
        public static List<Prop> store = new List<Prop>();

        public static int seed => (DateTime.Now.Year - 1) * 12 + (DateTime.Now.Month - 1) * 30 + (DateTime.Now.Day - 1) * 24 + DateTime.Now.Hour;
        private static int prevSeed;
        public static int favoribility = 0;
        public static int maxCount = 20;

        public static void LoadDealer(Dealer dealer)
        {
            favoribility = dealer.favoribility;
            prevSeed = dealer.seed;
            if (seed != prevSeed)
            {
                RefreshStore();
                prevSeed = seed;
            }
            else store = dealer.store;
            foreach (var prop in dealer.store) Debug.Log($"商人成功加进商品{prop.name}");
        }

///////////////////////////////////////////////////////////////////////////////////////////
        /// <summary>
        /// 只能用做玩家行为，因为内含除get外其他逻辑
        /// </summary>
        public static List<Prop> GetStore()
        {
            if (prevSeed != seed)
            {
                RefreshStore();
                prevSeed = seed;
            }
            return store;
        }

        public static void RemoveStoreByIndex(int index)
        {
            GetStore().RemoveAt(index);
            JsonData.SaveDealer();
        }
///////////////////////////////////////////////////////////////////////////////////////////

        private static void RefreshStore()
        {
            if (prevSeed == seed) return; //做进一步检查，本质没啥用，refresh之前就已经在检查了
            PropOccurProbability.Build();
            store = GenerateStore(seed, favoribility, maxCount);
            JsonData.SaveDealer();
            Debug.Log("商人更新");
        }

        private static List<Prop> GenerateStore(int seedVal, int favor, int count)
        {
            if(favor > 100) favor = 100;
            int weightCommon = 100;
            int weightRare   = favor / 2 +1;
            int weightLegend = favor / 10 +1;
            int total        = weightCommon + weightRare + weightLegend;

            var result = new List<Prop>();
            for (int i = 0; i < count; i++)
            {
                // seed 决定稀有度抽取结果和道具选取，favoribility 决定权重分布
                int rarityHash = Hash(seedVal, i)         & 0x7fffffff;
                int pickHash   = Hash(seedVal, i + count) & 0x7fffffff;

                int roll = rarityHash % total;
                Type[] pool;
                if (roll < weightLegend && PropOccurProbability.LegendProps.Length > 0)
                    pool = PropOccurProbability.LegendProps;
                else if (roll < weightLegend + weightRare && PropOccurProbability.RareProps.Length > 0)
                    pool = PropOccurProbability.RareProps;
                else
                    pool = PropOccurProbability.CommonProps;

                if (pool == null || pool.Length == 0) continue;

                var type = pool[pickHash % pool.Length];
                Prop prop = (Prop)Activator.CreateInstance(type);

                if (prop is Coin) continue;
                
                result.Add(prop);
                Debug.Log($"商店上架{prop.name}");
            }
            return result;
        }

        // 将 seed 与槽位 index 混合，产生确定性整数
        private static int Hash(int s, int index)
        {
            unchecked
            {
                int h = s ^ (index * (int)0x9e3779b9);
                h ^= h >> 16;
                h *= (int)0x85ebca6b;
                h ^= h >> 13;
                h *= (int)0xc2b2ae35;
                h ^= h >> 16;
                return h;
            }
        }

        public static void InitDealer()
        {
            JsonData.LoadDealer();
        }

        public static void AddFavoribility(int value)
        {
            favoribility += value;
            JsonData.SaveDealer();
        }
    }

    /// <summary>
    /// 只用于保存数据
    /// </summary>
    public class Dealer
    {
        public int seed;
        public List<Prop> store;
        public int favoribility;
        public Dealer(int seed,List<Prop> store,int favoribility)
        {
            this.seed = seed;
            this.store = store;
            this.favoribility = favoribility;
        }
    }
}
