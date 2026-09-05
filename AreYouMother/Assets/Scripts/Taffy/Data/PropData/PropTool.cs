using System.Collections.Generic;
using UnityEngine;

namespace Taffy.Data.PropData
{
    public static class PropTool
    {
        public static Prop GetRandomProp(this List<Prop> propList)
        {
            return propList[Random.Range(0, propList.Count)];
        }

        public static List<PropJson> ToJson(this List<Prop> propList)
        {
            if (propList is null) return null;
            List<PropJson> jsonList = new List<PropJson>();
            foreach (var prop in propList)
            {
                jsonList.Add(prop.ToJson());
            }
            return jsonList;
        }

        public static List<Prop> DeJson(this List<PropJson> propList)
        {
            if(propList is null) return null;
            
            List<Prop> list = new List<Prop>();
            foreach (var prop in propList)
            {
                var p = prop.DeJson();
                if (p != null) list.Add(p);   // 注册表里找不到的道具跳过，防止 null 混进列表导致后续序列化空引用
            }
            return list;
        }
    }
}
