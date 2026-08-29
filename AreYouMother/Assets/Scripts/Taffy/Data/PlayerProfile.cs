using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using Taffy.Data.PropData;

namespace Taffy.Data
{
    [Serializable]
    public class PlayerProfile
    {
        public string ID = "";
        public int maxHP = 0;
        public int maxMP = 0;
        public int bagSize = 0;
        [JsonIgnore]
        public List<Prop> bag =  new List<Prop>();
        
        public PlayerProfile(string ID = "", int maxHP = 0, int maxMP = 0, int bagSize = 0)
        {
            this.ID = ID;
            this.maxHP = maxHP;
            this.maxMP = maxMP;
            this.bagSize = bagSize;
        }
    }
}
