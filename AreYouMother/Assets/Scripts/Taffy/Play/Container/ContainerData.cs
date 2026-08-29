using System.Collections.Generic;
using NUnit.Framework;
using Taffy.Data;
using Taffy.Data.PropData;
using UnityEngine;

namespace Taffy.Play.Container
{
    public class ContainerData : MonoBehaviour
    {
        public int length = 0;
        public List<Prop> container = new List<Prop>();
        public ContainerType type = ContainerType.Common;
        public Prop GetPropByIndex(int index)
        {
            if(index >= container.Count) return null;
            return container[index];
        }

        public int GetCount()
        {
            return container.Count;
        }

        public void RemovePropByIndex(int index)
        {
            if (index >= container.Count) return;
            container.RemoveAt(index);
        }
        
        public List<Prop> GetAllProps()
        {
            return container;
        }

        public void AddProp(Prop prop)
        {
            
        }
    }
}
