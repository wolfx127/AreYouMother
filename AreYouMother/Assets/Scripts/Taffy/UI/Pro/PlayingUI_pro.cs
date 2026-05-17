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
        }
    }
}
