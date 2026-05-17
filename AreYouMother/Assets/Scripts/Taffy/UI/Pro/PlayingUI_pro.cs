using Taffy.OverAllManager;
using UnityEngine;

namespace Taffy.UI.Pro
{
    public class PlayingUI_pro
    {
        
        private int maxHP_playerA = 0;
        private int maxHP_playerB = 0;
        private int maxMP_playerA = 0;
        private int maxMP_playerB = 0;
        private int curHP_playerA = 0;
        private int curHP_playerB = 0;
        private int curMP_playerA = 0;
        private int curMP_playerB = 0;

        public void SubscribeEvents()
        {
            EventBus.Subscribe<GetPlayersHPandMPEvent>(GetPlayersHPandMP);
        }

        public void UnSubscribeEvents()
        {
            EventBus.Unsubscribe<GetPlayersHPandMPEvent>(GetPlayersHPandMP);
        }

        private void GetPlayersHPandMP(GetPlayersHPandMPEvent evt)
        {
            maxHP_playerA = evt.HP_playerA;
            maxHP_playerB = evt.HP_playerB;
            maxMP_playerA = evt.MP_playerA;
            maxMP_playerB = evt.MP_playerB;
            Debug.Log("playingUI成功拿到player的数值");
            curHP_playerA = maxHP_playerA;
            curHP_playerB = maxHP_playerB;
            curMP_playerA = maxMP_playerA;
            curMP_playerB = maxMP_playerB;
        }

        public string InfoNum_playerA()
        {
            return $"HP:{curHP_playerA}/{maxHP_playerA}"+'\n'+$"MP:{curMP_playerA}/{maxMP_playerA}";
        }
        
        public string InfoNum_playerB()
        {
            return $"HP:{curHP_playerB}/{maxHP_playerB}"+'\n'+$"MP:{curMP_playerB}/{maxMP_playerB}";
        }
    }
}
