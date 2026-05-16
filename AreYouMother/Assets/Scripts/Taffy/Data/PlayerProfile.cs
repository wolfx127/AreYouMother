namespace Taffy.Data
{
    public class PlayerProfile
    {
        public string ID = "";
        public int maxHP = 0;
        public int maxMP = 0;
        public int bagSize = 0;
        
        public PlayerProfile(string ID = "", int maxHP = 0, int maxMP = 0, int bagSize = 0)
        {
            this.ID = ID;
            this.maxHP = maxHP;
            this.maxMP = maxMP;
            this.bagSize = bagSize;
        }
    }
}
