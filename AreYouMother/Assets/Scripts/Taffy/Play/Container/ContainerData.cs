using System.Collections.Generic;
using UnityEngine;

namespace Taffy.Data
{
    public class ContainerData:MonoBehaviour
    {
        private List<Prop> container = new List<Prop>();
        public int length = 5;
        public string name => gameObject.name;
        
        public bool AddProp(Prop prop)
        {
            if (container.Count >= length) return false;
            container.Add(prop);
            Debug.Log($"{gameObject.name} 加进一个 {prop.name}");
            return true;
        }
        public bool RemoveProp(Prop prop)
        {
            if(!container.Contains(prop))  return false;
            container.Remove(prop);
            return true;
        }

        public bool RemovePropByIndex(int index)
        {
            if(container.Count <= index) return false;
            container.RemoveAt(index);
            return true;
        }

        public Prop GetPropByIndex(int index)
        {
            if(!IsHereHasProp(index) || container[index]  == null) return null;
            return container[index];
        }

        public bool IsHereHasProp(int index)
        {
            if(index >= container.Count) return false;
            return true;
        }
        
        public List<Prop> GetAllProps()
        {
            return container;
        }
        
        public int GetCount() => container.Count;
    }
}
