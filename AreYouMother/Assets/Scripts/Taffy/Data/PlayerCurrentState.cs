using System.Collections.Generic;
using Taffy.Data.PropData;

namespace Taffy.Data
{
    public class PlayerCurrentState
    {
        public int curHP = 0;
        public int curMP = 0;
        public List<Prop> bag =  new List<Prop>();

        public int ATK = 0;
        public int DEF = 0;

        public bool isDead = false;
    }
}
