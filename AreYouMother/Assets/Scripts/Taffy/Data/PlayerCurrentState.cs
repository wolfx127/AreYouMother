using System.Collections.Generic;

namespace Taffy.Data
{
    public class PlayerCurrentState
    {
        public string ID = "";
        public int curHP = 0;
        public int curMP = 0;
        public List<Prop> bag =  new List<Prop>();

        public bool isDead = false;
    }
}
