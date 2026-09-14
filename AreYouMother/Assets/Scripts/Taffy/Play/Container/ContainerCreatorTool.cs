using System;
using System.Collections.Generic;
using Taffy.Data;
using Taffy.Data.PropData;
using UnityEngine;

namespace Taffy.Play.Container
{
    public static class ContainerCreatorTool
    {
        private static readonly Dictionary<(ContainerType, Rarity), List<Prop>> buckets =
            new Dictionary<(ContainerType, Rarity), List<Prop>>();

        public static void Build()
        {
            Debug.Log("ContainerCreatorTool 开始分桶");

            buckets.Clear();

            foreach (ContainerType type in Enum.GetValues(typeof(ContainerType)))
            foreach (Rarity rarity in Enum.GetValues(typeof(Rarity)))
                buckets[(type, rarity)] = new List<Prop>();

            foreach (var so in PropList.propSOList)
            {
                var p = new Prop(so);
                buckets[(so.containerType, p.rarity)].Add(p);
            }

            int total = 0;
            int empty = 0;
            foreach (var kv in buckets)
            {
                total += kv.Value.Count;
                if (kv.Value.Count == 0)
                {
                    Debug.LogWarning($"ContainerCreatorTool 空桶：{kv.Key.Item1}箱 × {kv.Key.Item2}，没有这种组合的道具");
                    empty++;
                }
            }
            Debug.Log($"ContainerCreatorTool 分桶完成，共 {total} 个Prop，{buckets.Count} 个桶，其中空桶 {empty} 个");
        }

        public static List<Prop> GetUnionList(ContainerType containerType, Rarity rarity)
        {
            return buckets.GetValueOrDefault((containerType, rarity));
        }
    }
}
