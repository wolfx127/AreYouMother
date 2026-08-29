using System;
using System.Collections.Generic;
using Taffy.Data;
using Taffy.Data.PropData;
using Taffy.UI.Pro;
using Unity.VisualScripting;
using UnityEngine;

namespace Taffy.Home
{
    public static class WarehouseManager
    {
        private static List<Prop> warehouse = new List<Prop>();
        public static int property = 0;

        public static void AddProp(Prop prop)
        {
            warehouse.Add(prop);
            JsonData.SaveWarehouse();
            Debug.Log($"仓库成功加进道具{prop.name}");
        }

        public static void RemoveProp(Prop prop)
        {
            warehouse.Remove(prop);
            JsonData.SaveWarehouse();
            Debug.Log($"仓库移除道具{prop.name}");
        }

        public static void RemovePropByIndex(int index)
        {
            warehouse.RemoveAt(index);
            JsonData.SaveWarehouse();
        }

        public static List<Prop> GetWarehouse()
        {
            return warehouse;
        }

        public static Prop GetPropByIndex(int index)
        {
            return warehouse[index];
        }

        public static int GetWarehouseCount()
        {
            return warehouse.Count;
        }

        public static void ResetWarehouse()
        {
            warehouse =  new List<Prop>();
            property = 2000;
        }

        public static void AddProperty(int count)
        {
            property += count;
            JsonData.SaveWarehouse();
        }

        public static void MinusProperty(int count)
        {
            if (count > property) return;
            property -= count;
            JsonData.SaveWarehouse();
        }

        public static bool CanMinusProperty(int count)
        {
            return count <= property;
        }

        /// <summary>
        /// 仅做初始化，后续非必要情况不要调用
        /// </summary>
        /// <param name="jsonWarehouse"> 来自json的调用 </param>
        public static void LoadWarehouse(Warehouse jsonWarehouse)
        {
            ResetWarehouse();
            property = jsonWarehouse.property;
            if(property < 0) property = 0;
            foreach (var prop in jsonWarehouse.warehouse)
            {
                warehouse.Add(prop.DeJson());
                Debug.Log($"仓库成功加进道具{prop.name}");
            }
        }

        public static void InitWarehouse()
        {
            JsonData.LoadWarehouse();
        }
    }

    public class Warehouse
    {
        public int property = 0;
        public List<PropJson>  warehouse = new List<PropJson>();

        public Warehouse(int property, List<Prop> warehouse)
        {
            this.property = property;
            this.warehouse = warehouse.ToJson();
        }
    }
}
