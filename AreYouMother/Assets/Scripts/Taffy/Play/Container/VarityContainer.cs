using System;
using Taffy.Data;
using UnityEngine;
using Random = UnityEngine.Random;
using ContainerType = Taffy.Data.ContainerType;

namespace Taffy.Play.Container
{
    public class VarityContainer : MonoBehaviour
    {
        public ContainerData containerData;
        public ContainerType containerType = ContainerType.Common;
        string Name => gameObject.name[..gameObject.name.LastIndexOf("_Case")];

        private void Awake()
        {
            
        }

        private void Start()
        {
            try
            {
                if      (Name.Equals("Treat")) containerType = ContainerType.Cure;
                else if (Name.Equals("Weapon")) containerType = ContainerType.Weapon;
                else if (Name.Equals("Defence")) containerType = ContainerType.Armor;
                else if (Name.Equals("Insurance")) containerType = ContainerType.Treasure;
            }
            catch (Exception e) { Debug.Log(e); }

            containerData = gameObject.GetComponent<ContainerData>();
            FillContainer();
        }

        private void FillContainer()
        {
            int count = Random.Range(1, 6);
            for (int i = 0; i < count; i++)
            {
                PropRarity rarity = GetRandomRarity();
                Type t = PickType(rarity);
                if (t == null) continue;
                Prop prop = (Prop)Activator.CreateInstance(t);
                containerData.AddProp(prop);
            }
        }

        private Type PickType(PropRarity rarity)
        {
            // 有类型的箱子：从类型池里筛出对应稀有度，没有则降级到普通
            Type[] typePool = containerType switch
            {
                ContainerType.Weapon => PropOccurType.WeaponProp,
                ContainerType.Armor  => PropOccurType.ArmorProp,
                ContainerType.Cure   => PropOccurType.CureProp,
                _                    => null
            };

            if (typePool != null)
            {
                Type[] filtered = Array.FindAll(typePool, t =>
                {
                    var instance = (Prop)Activator.CreateInstance(t);
                    return instance.rarity == rarity;
                });
                if (filtered.Length == 0)
                    filtered = Array.FindAll(typePool, t =>
                    {
                        var instance = (Prop)Activator.CreateInstance(t);
                        return instance.rarity == PropRarity.Common;
                    });
                if (filtered.Length == 0) return null;
                return filtered[Random.Range(0, filtered.Length)];
            }

            // 无类型箱子（Common/Insurance）：直接从稀有度池取
            Type[] rarityPool = rarity switch
            {
                PropRarity.Common => PropOccurProbability.CommonProps,
                PropRarity.Rare   => PropOccurProbability.RareProps,
                PropRarity.Legend => PropOccurProbability.LegendProps,
                _                 => PropOccurProbability.CommonProps
            };
            if (rarityPool == null || rarityPool.Length == 0) return null;
            return rarityPool[Random.Range(0, rarityPool.Length)];
        }

        private PropRarity GetRandomRarity()
        {
            float roll = Random.Range(0f, 100f);
            return containerType switch
            {
                ContainerType.Common   => roll < 80f ? PropRarity.Common : roll < 97f ? PropRarity.Rare : PropRarity.Legend,
                ContainerType.Cure     => roll < 75f ? PropRarity.Common : roll < 95f ? PropRarity.Rare : PropRarity.Legend,
                ContainerType.Weapon   => roll < 55f ? PropRarity.Common : roll < 90f ? PropRarity.Rare : PropRarity.Legend,
                ContainerType.Armor    => roll < 55f ? PropRarity.Common : roll < 90f ? PropRarity.Rare : PropRarity.Legend,
                ContainerType.Treasure => roll < 30f ? PropRarity.Common : roll < 65f ? PropRarity.Rare : PropRarity.Legend,
                _                      => roll < 80f ? PropRarity.Common : roll < 97f ? PropRarity.Rare : PropRarity.Legend,
            };
        }
    }
}
