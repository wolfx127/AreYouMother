using System;
using System.Collections.Generic;
using Taffy.Data;
using Taffy.Data.PropData;
using Taffy.OverAllManager;
using Taffy.UI.Pro;
using Unity.VisualScripting;
using UnityEditor.AddressableAssets.Settings;
using UnityEngine;
using UnityEngine.AddressableAssets;
using EventBus = Taffy.OverAllManager.EventBus;

namespace Taffy.Home
{
    public static class WarehouseManager
    {
        private static List<Prop> warehouse = new List<Prop>();
        public static int property = 0;

        public static event Action UpdatePropertyEvent;

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
            Debug.Log("开始加载默认仓库");
            var handler = Addressables.LoadAssetsAsync<WarehouseSO>("WarehouseSO");
            handler.WaitForCompletion();
            warehouse.Clear();
            foreach (var element in handler.Result)
            {
                if (element.name == "DefaultWarehouseSO")
                {
                    foreach (var propSO in element.warehouse)
                    {
                        if(propSO != null)
                            warehouse.Add(new Prop(propSO));
                    }
                    property = element.property;
                    break;
                }
            }
            
            JsonData.SaveWarehouse();
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
            property = jsonWarehouse.property;
            if(property < 0) property = 0;
            foreach (var prop in jsonWarehouse.warehouse)
            {
                var p = prop.DeJson();
                if (p != null) warehouse.Add(p);   // 注册表里找不到的道具跳过，防止 null 混入
                Debug.Log($"仓库成功加进道具{prop.name}");
            }
            JsonData.SaveWarehouse();   // 读到的数据回写一次，防止旧格式残留
        }

        public static void InitWarehouse()
        {
            JsonData.LoadWarehouse();
            Subscribe();
        }

        private static void Subscribe()
        {
            EventBus.Subscribe<ExitGameEvent>(SaveWarehouse);
        }

        private static void SaveWarehouse(ExitGameEvent evt)
        {
            JsonData.SaveWarehouse();
        }

        public static void SaveWarehouse()
        {
            JsonData.SaveWarehouse();
        }
    }

    public class Warehouse
    {
        public int property = 0;
        public List<PropJson>  warehouse = new List<PropJson>();

        public Warehouse() { }

        public Warehouse(int property, List<Prop> warehouse)
        {
            this.property = property;
            this.warehouse = warehouse.ToJson();
        }
    }

    [CreateAssetMenu(menuName = "Warehouse/WarehouseSO")]
    public class WarehouseSO : ScriptableObject
    {
        public int property = 0;
        public List<PropSO> warehouse = new List<PropSO>();
    }
}
