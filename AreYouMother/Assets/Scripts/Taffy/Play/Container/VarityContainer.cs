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
        public string Name => gameObject.name[..2];

        private void Awake()
        {
            
        }

        private void Start()
        {
            try
            {
                if      (Name.Equals("医疗")) containerType = ContainerType.Cure;
                else if (Name.Equals("武器")) containerType = ContainerType.Weapon;
                else if (Name.Equals("防具")) containerType = ContainerType.Armor;
                else if (Name.Equals("保险")) containerType = ContainerType.Treasure;
            }
            catch (Exception e) { Debug.Log(e); }

            containerData = gameObject.GetComponent<ContainerData>();
            FillContainer();
        }

        private void FillContainer()
        {
            int count = Random.Range(0, 5);
            int filled = 0;
            int maxAttempts = count * 20;

            for (int attempt = 0; filled < count && attempt < maxAttempts; attempt++)
            {
                PropRarity rarity = GetRandomRarity();
                Type[] rarityPool = rarity switch
                {
                    PropRarity.普通 => PropOccurProbability.PropRarityRegistry.CommonProps,
                    PropRarity.稀有 => PropOccurProbability.PropRarityRegistry.RareProps,
                    PropRarity.传说 => PropOccurProbability.PropRarityRegistry.LegendProps,
                    _              => PropOccurProbability.PropRarityRegistry.CommonProps
                };

                if (rarityPool == null || rarityPool.Length == 0) continue;

                Type t = rarityPool[Random.Range(0, rarityPool.Length)];
                if (!IsAccepted(t)) continue;

                Prop prop = (Prop)Activator.CreateInstance(t);
                if (containerData.AddProp(prop)) filled++;
            }
        }

        private bool IsAccepted(Type t)
        {
            return containerType switch
            {
                ContainerType.Weapon   => typeof(IWeapon).IsAssignableFrom(t),
                ContainerType.Armor    => typeof(IDefend).IsAssignableFrom(t),
                ContainerType.Cure     => typeof(ICure).IsAssignableFrom(t),
                ContainerType.Treasure => typeof(ITreasure).IsAssignableFrom(t),
                ContainerType.Common   => true,
                _                      => true
            };
        }

        private PropRarity GetRandomRarity()
        {
            float roll = Random.Range(0f, 100f);
            return containerType switch
            {
                ContainerType.Common   => roll < 80f ? PropRarity.普通 : roll < 97f ? PropRarity.稀有 : PropRarity.传说,
                ContainerType.Cure     => roll < 75f ? PropRarity.普通 : roll < 95f ? PropRarity.稀有 : PropRarity.传说,
                ContainerType.Weapon   => roll < 55f ? PropRarity.普通 : roll < 90f ? PropRarity.稀有 : PropRarity.传说,
                ContainerType.Armor    => roll < 55f ? PropRarity.普通 : roll < 90f ? PropRarity.稀有 : PropRarity.传说,
                ContainerType.Treasure => roll < 30f ? PropRarity.普通 : roll < 65f ? PropRarity.稀有 : PropRarity.传说,
                _                      => roll < 80f ? PropRarity.普通 : roll < 97f ? PropRarity.稀有 : PropRarity.传说,
            };
        }
    }
}
