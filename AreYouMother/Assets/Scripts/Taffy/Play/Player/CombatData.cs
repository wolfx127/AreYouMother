namespace Taffy.Play.Player
{
    public class CombatData
    {
        public int maxHP;
        public int maxMP;
        public int HP;
        public int MP;
        public int ATK;
        public int DEF;

        public CombatData(int HP, int MP, int ATK, int DEF)
        {
            maxHP = HP;
            maxMP = MP;
            this.HP = HP;
            this.MP = MP;
            this.ATK = ATK;
            this.DEF = DEF;
        }
    }
}
