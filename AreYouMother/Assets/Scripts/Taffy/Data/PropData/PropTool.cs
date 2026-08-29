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
                list.Add(prop.DeJson());
            }
            return list;
        }
    }
}
