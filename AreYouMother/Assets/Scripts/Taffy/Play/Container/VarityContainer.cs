using System;
using Taffy.Data;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Taffy.Play.Container
{
    public enum ContainerType
    {
        Common,
        Cure,
        Weapon,
        Armor,
        Treasure
    }

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
                Rarity rarity = GetRandomRarity();
                Type t = PickType(rarity);
                if (t == null) continue;
                Prop prop = (Prop)Activator.CreateInstance(t);
                containerData.AddProp(prop);
            }
        }

        private Type PickType(Rarity rarity)
        {
//TODO            
            return typeof(int);
        }

        private Rarity GetRandomRarity() 
        {
            float roll = Random.Range(0f, 100f);
            return containerType switch
            {
                ContainerType.Common   => roll < 80f ? Rarity.Common : roll < 97f ? Rarity.Rare : Rarity.Legendary,
                ContainerType.Cure     => roll < 75f ? Rarity.Common : roll < 95f ? Rarity.Rare : Rarity.Legendary,
                ContainerType.Weapon   => roll < 55f ? Rarity.Common : roll < 90f ? Rarity.Rare : Rarity.Legendary,
                ContainerType.Armor    => roll < 55f ? Rarity.Common : roll < 90f ? Rarity.Rare : Rarity.Legendary, 
                ContainerType.Treasure => roll < 30f ? Rarity.Common : roll < 65f ? Rarity.Rare : Rarity.Legendary,
                _                      => roll < 80f ? Rarity.Common : roll < 97f ? Rarity.Rare : Rarity.Legendary,
            };
        }
    }
}
