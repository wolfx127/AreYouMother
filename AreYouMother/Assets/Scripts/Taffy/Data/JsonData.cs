using System;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using Taffy.Data;
using Taffy.Home;
using UnityEngine;

public static class JsonData
{
    private static string PlayerFilePath => Path.Combine(Path.GetDirectoryName(Application.dataPath), "players.json");
    private static string WarehouseFilePath => Path.Combine(Path.GetDirectoryName(Application.dataPath), "warehouse.json");
    private static string DealerFilePath => Path.Combine(Path.GetDirectoryName(Application.dataPath), "dealer.json");

    private static readonly JsonSerializerSettings Settings = new JsonSerializerSettings
    {
        TypeNameHandling    = TypeNameHandling.All,
        Formatting          = Formatting.Indented,
        NullValueHandling   = NullValueHandling.Include,
        ConstructorHandling = ConstructorHandling.AllowNonPublicDefaultConstructor,
        Error = (_, e) =>
        {
            Debug.LogWarning($"[JsonData] 反序列化警告（已跳过该字段）: {e.ErrorContext.Error.Message}");
            e.ErrorContext.Handled = true;
        }
    };

    private struct SavePlayerData
    {
        public PlayerProfile playerA;
        public PlayerProfile playerB;
    }

    public static void SavePlayer(PlayerProfile player1, PlayerProfile player2)
    {
        string json = JsonConvert.SerializeObject(new SavePlayerData { playerA = player1, playerB = player2 }, Settings);
        File.WriteAllText(PlayerFilePath, json);
        Debug.Log("保存玩家数据成功");
    }

    public static (PlayerProfile player1, PlayerProfile player2) LoadPlayer()
    {
        if (!File.Exists(PlayerFilePath))
        {
            var defaults = (new PlayerProfile("A", 100, 100, 20),
                            new PlayerProfile("B", 100, 100, 20));
            SavePlayer(defaults.Item1, defaults.Item2);
            return defaults;
        }

        try
        {
            SavePlayerData data = JsonConvert.DeserializeObject<SavePlayerData>(File.ReadAllText(PlayerFilePath), Settings);
            Debug.Log("加载玩家数据成功");
            return (data.playerA, data.playerB);
        }
        catch (System.Exception e)
        {
            Debug.LogError($"加载玩家数据失败，返回默认数据: {e.Message}");
            return (new PlayerProfile("A", 100, 100, 20),
                    new PlayerProfile("B", 100, 100, 20));
        }
    }

    public static void SaveWarehouse()
    {
        string json = JsonConvert.SerializeObject(new Warehouse(WarehouseManager.property,WarehouseManager.GetWarehouse()),Settings);
        File.WriteAllText(WarehouseFilePath, json);
        Debug.Log("保存仓库数据成功");
    }

    public static void LoadWarehouse()
    {
        if (!File.Exists(WarehouseFilePath))
        {
            SaveWarehouse();
            WarehouseManager.ResetWarehouse();
            return;
        }

        try
        {
            Warehouse warehouse = JsonConvert.DeserializeObject<Warehouse>(File.ReadAllText(WarehouseFilePath), Settings);
            WarehouseManager.LoadWarehouse(warehouse);
            Debug.Log("加载仓库数据成功");
        }
        catch (Exception e)
        {
            Debug.Log($"e : {e.Message}");
        }
    }
    
    public static void SaveDealer()
    {
        string json = JsonConvert.SerializeObject(new Dealer(DealerManager.seed,DealerManager.store,DealerManager.favoribility),Settings);
        File.WriteAllText(DealerFilePath, json);
        Debug.Log("保存商人数据成功");
    }

    public static void LoadDealer()
    {
        if (!File.Exists(DealerFilePath))
        {
            SaveDealer();
            return;
        }

        try
        {
            Dealer dealer = JsonConvert.DeserializeObject<Dealer>(File.ReadAllText(DealerFilePath), Settings);
            DealerManager.LoadDealer(dealer);
            Debug.Log("加载商人数据成功");
        }
        catch (Exception e)
        {
            Debug.Log($"e : {e.Message}");
        }
    }
}
