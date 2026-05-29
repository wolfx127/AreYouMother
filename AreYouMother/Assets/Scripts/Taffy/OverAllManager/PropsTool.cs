using System.IO;
using Taffy.Data;
using UnityEngine;

namespace Taffy.OverAllManager
{
    public static class PropsTool
    {
        public static string ToLocalizedString(this PropRarity rarity) => rarity switch
        {
            PropRarity.Common => "普通",
            PropRarity.Rare   => "稀有",
            PropRarity.Legend => "传说",
            _                 => rarity.ToString()
        };

        public static Texture2D GetPropImage(Prop prop)
        {
            if (prop == null || string.IsNullOrEmpty(prop.imagePath)) return null;
            string path = Path.Combine(Application.streamingAssetsPath, "PropImages", prop.imagePath);
            if (!File.Exists(path))
            {
                Debug.LogWarning($"[PropsTool] 找不到图片: {path}");
                path = Path.Combine(Application.streamingAssetsPath, "PropImages", "NullProp.png");
            }
            var tex = new Texture2D(2, 2);
            if (!tex.LoadImage(File.ReadAllBytes(path)))
            {
                Debug.LogWarning($"[PropsTool] 图片解码失败: {path}");
                return null;
            }
            return tex;
        }
    }
}
