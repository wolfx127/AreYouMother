using System.IO;
using Newtonsoft.Json;
using Taffy.Data;
using UnityEngine;

public static class JsonData
{
    private static string FilePath => Path.Combine(Path.GetDirectoryName(Application.dataPath), "players.json");

    private static readonly JsonSerializerSettings Settings = new JsonSerializerSettings
    {
        TypeNameHandling = TypeNameHandling.All,
        Formatting = Formatting.Indented
    };

    private struct SaveData
    {
        public PlayerProfile playerA;
        public PlayerProfile playerB;
    }

    public static void Save(PlayerProfile player1, PlayerProfile player2)
    {
        string json = JsonConvert.SerializeObject(new SaveData { playerA = player1, playerB = player2 }, Settings);
        File.WriteAllText(FilePath, json);
        Debug.Log("保存数据成功");
    }

    public static (PlayerProfile player1, PlayerProfile player2) Load()
    {
        if (!File.Exists(FilePath))
        {
            var defaults = (new PlayerProfile("A", 100, 100, 10),
                            new PlayerProfile("B", 100, 100, 10));
            Save(defaults.Item1, defaults.Item2);
            return defaults;
        }
        SaveData data = JsonConvert.DeserializeObject<SaveData>(File.ReadAllText(FilePath), Settings);
        Debug.Log("加载数据成功");
        return (data.playerA, data.playerB);
    }
}
