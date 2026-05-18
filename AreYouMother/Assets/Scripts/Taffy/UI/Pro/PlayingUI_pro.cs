using Taffy.OverAllManager;
using Taffy.Play.Player;
using UnityEngine;

namespace Taffy.UI.Pro
{
    public class PlayingUI_pro
    {
        PlayerCurrentStateController pcsc => PlayerCurrentStateController.Instance;
        
        public string InfoNum_playerA()
        {
            return $"HP:{pcsc.GetCurHP_A()}/{pcsc.GetMaxHP_A()}"+'\n'+$"MP:{pcsc.GetCurMP_A()}/{pcsc.GetMaxMP_A()}";
        }
        
        public string InfoNum_playerB()
        {
            return $"HP:{pcsc.GetCurHP_B()}/{pcsc.GetMaxHP_B()}"+'\n'+$"MP:{pcsc.GetCurMP_B()}/{pcsc.GetMaxMP_B()}";
        }
    }
}
