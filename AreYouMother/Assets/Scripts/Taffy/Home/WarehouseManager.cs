using System;
using System.Collections.Generic;
using Taffy.Data;
using Taffy.UI.Pro;
using Unity.VisualScripting;

namespace Taffy.Home
{
    public static class WarehouseManager
    {
        private static List<Prop> warehouse = new List<Prop>();

        public static void AddProp(Prop prop)
        {
            warehouse.Add(prop);
        }

        public static void RemoveProp(Prop prop)
        {
            warehouse.Remove(prop);
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
        }

        /// <summary>
        /// 仅做初始化，后续非必要情况不要调用
        /// </summary>
        /// <param name="jsonWarehouse"> 来自json的调用 </param>
        public static void LoadWarehouse(List<Prop> jsonWarehouse)
        {
            ResetWarehouse();
            foreach (var prop in jsonWarehouse) warehouse.Add(prop);
        }

        public static void InitWarehouse()
        {
            JsonData.LoadWarehouse();
        }
    }
}
