using System.Collections.Generic;
using Microsoft.Unity.VisualStudio.Editor;
using Unity.Burst.Intrinsics;
using UnityEngine;
using UnityEngine.Rendering;

namespace Taffy.Data
{
    public enum PropType
    {
        Default,
        Treasure,
        Weapon,
        Armor,
        Assumption,
        Buff,
        Debuff
    }

    public enum Rarity
    {
        Common,
        Unusual,
        Rare,
        Legendary,
    }

    public class Prop
    {
        public string name;
        public PropType[] type = new PropType[4] { PropType.Default ,PropType.Default,PropType.Default,PropType.Default};
        public int[] value = new int[4]{0,0,0,0};
        public string description;
        public int price = 0;
        public Rarity rarity = Rarity.Common;
        public Texture2D image;
    }

    [CreateAssetMenu(fileName = "PropSO" , menuName = "Create/PropSO")]
    public class PropSO : ScriptableObject
    {
        public string name;
        public PropType[] type = new PropType[4] { PropType.Default ,PropType.Default,PropType.Default,PropType.Default};
        public int[] value = new int[4]{0,0,0,0};
        public string description;
        public int price = 0;
        public Rarity rarity = Rarity.Common;
        public Texture2D image;
    }
}