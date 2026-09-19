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
            buckets.Clear();
            if (PropList.propSOList.Count == 0) PropList.BuildList();

            foreach (ContainerType type in Enum.GetValues(typeof(ContainerType)))
            foreach (Rarity rarity in Enum.GetValues(typeof(Rarity)))
                buckets[(type, rarity)] = new List<Prop>();

            int total = 0;
            foreach (var p in PropList.propList)
            {
                buckets[(p.containerType, p.rarity)].Add(p.Clone());
                total++;
            }
            builtTotal = total;
            Debug.Log($"ContainerCreatorTool 分桶完成，共 {total} 个Prop");
        }

        private static int builtTotal = 0;

        public static List<Prop> GetUnionList(ContainerType containerType, Rarity rarity)
        {
            if (buckets.Count == 0 || builtTotal == 0) Build();
            return buckets.GetValueOrDefault((containerType, rarity));
        }
    }
}
