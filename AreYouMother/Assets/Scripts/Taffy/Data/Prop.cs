using System;
using System.Collections.Generic;
using Microsoft.Unity.VisualStudio.Editor;
using Unity.Burst.Intrinsics;
using UnityEngine;
using UnityEngine.Rendering;

namespace Taffy.Data
{
    public enum PropType
    {
        [InspectorName("默认")]Default,
        [InspectorName("收藏品")]Treasure,
        [InspectorName("可近战")]Close_Attack,
        [InspectorName("可远攻")]Remote_Attack,
        [InspectorName("可防御")]Defend,
        [InspectorName("可治疗_一次消耗性")]AddBlood,
        [InspectorName("可回蓝_一次消耗性")]AddSkill,
        [InspectorName("可提升血量上限_一次消耗性")]AddMaxBlood,
        [InspectorName("可提升蓝条上限_一次消耗性")]AddMaxSkill,
        [InspectorName("可提升商人好感度_一次消耗性")]AddFavorability,
        [InspectorName("攻击使敌人中毒一段时间")]Poison,
        [InspectorName("解自身负面效果的剩余时间_一次消耗性")]Detoxify,
    }

    public enum Rarity
    {
        Common,
        Unusual,
        Rare,
        Legendary,
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
        public string description;
        public int price = 0;
        public Rarity rarity = Rarity.Common;
        public Texture2D image;
    }
}