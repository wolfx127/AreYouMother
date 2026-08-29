using System;
using System.Collections.Generic;
using Taffy.Data;
using Taffy.Data.PropData;
using Taffy.OverAllManager;
using UnityEngine;

namespace Taffy.Home
{
    public static class DealerManager
    {
        /// <summary>
        /// 只能保存数据时调用
        /// </summary>
        public static List<Prop> store = new List<Prop>();

        // seed = 时间窗口(20分钟) 与 好感度 的混合，任一方变化 seed 就变
        private static long seed => Hash((int)(DateTimeOffset.Now.ToUnixTimeSeconds() / 1200), favoribility);
        private static long prevSeed;
        private static int favoribility = 0;
        public static int maxCount = 20;

        public static void LoadDealer(Dealer dealer)
        {
            favoribility = dealer.favoribility;
            if (seed != dealer.seed)
                UpdateDealer();
            else
                store = dealer.store.DeJson();
            if (store == null) store = new List<Prop>();
            foreach (var prop in store) Debug.Log($"商人成功加进商品{prop.name}");
        }

///////////////////////////////////////////////////////////////////////////////////////////
        /// <summary>
        /// 只能用做玩家行为，因为内含除get外其他逻辑
        /// </summary>
        public static List<Prop> GetStore()
        {
            UpdateDealer(); // 时间或好感度变了才会真正重刷
            return store;
        }

        public static void RemoveStoreByIndex(int index)
        {
            GetStore().RemoveAt(index);
            JsonData.SaveDealer();
            EventBus.Publish(new DealerUpdateEvent());
        }
///////////////////////////////////////////////////////////////////////////////////////////

        public static void UpdateDealer()
        {
            if (prevSeed == seed) return; // 时间和好感度都没变，不重刷
            
            store = GenerateStore(seed, favoribility, maxCount);
            prevSeed = seed;
            JsonData.SaveDealer();
            EventBus.Publish(new DealerUpdateEvent());
            Debug.Log("商人更新");
        }

        private static List<Prop> GenerateStore(long seedVal, int favor, int count)
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
                long rarityHash = Hash(seedVal, i)         & 0x7fffffff;
                long pickHash   = Hash(seedVal, i + count) & 0x7fffffff;

                long roll = rarityHash % total;
                Rarity rarity;
                if (roll < weightCommon) rarity = Rarity.Common;
                else if (roll < weightCommon + weightRare) rarity = Rarity.Rare;
                else rarity = Rarity.Legend;

                var pool = PropList.propList.FindAll(p => p.rarity == rarity);
                if (pool.Count == 0) pool = PropList.propList; // 该稀有度没货，用全道具兜底，保证填满
                if (pool.Count == 0) break;                    // 注册表都空了，没得填

                result.Add(pool[(int)pickHash % pool.Count].Clone());
                
            }
            return result;
        }

        // 将 seed 与槽位 index 混合，产生确定性整数
        private static long Hash(long s, int index)
        {
            unchecked
            {
                long h = s ^ (index * (int)0x9e3779b9);
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
            if(favoribility > 100) favoribility = 100;
            UpdateDealer(); // 好感度变了 seed 跟着变，内部检测到就会换货+存档+发事件
        }
        
        public static int GetFavoribility()
        {
            return favoribility;
        }
        
        public static long GetPrevSeed()
        {
            return prevSeed;
        }
    }

    /// <summary>
    /// 只用于保存数据
    /// </summary>
    public class Dealer
    {
        public long seed;
        public List<PropJson> store;
        public int favoribility;
        public Dealer(long seed, List<Prop> store,int favoribility)
        {
            this.seed = seed;
            this.store = store.ToJson();
            this.favoribility = favoribility;
        }

        public Dealer()
        {
            this.seed = DealerManager.GetPrevSeed();
        }
    }
}
