using Taffy.OverAllManager;
using Taffy.Play.Player;
using UnityEngine;

namespace Taffy.UI.Pro
{
    public class PlayingUI_pro
    {
        /// <summary>
        /// 对外接口应当仅用作事件注册
        /// </summary>
        public PlayerCurrentStateController pcsc => PlayerCurrentStateController.Instance;

        public string InfoNum_playerA()
        {
            return $"HP:{pcsc.GetCurHP_A()}/{pcsc.GetMaxHP_A()}"+'\n'+$"MP:{pcsc.GetCurMP_A()}/{pcsc.GetMaxMP_A()}";
        }

        public string InfoNum_playerB()
        {
            return $"HP:{pcsc.GetCurHP_B()}/{pcsc.GetMaxHP_B()}"+'\n'+$"MP:{pcsc.GetCurMP_B()}/{pcsc.GetMaxMP_B()}";
        }

        public float HPPercent_A() => pcsc.GetHPPercent_A() * 100f;
        public float HPPercent_B() => pcsc.GetHPPercent_B() * 100f;
        public float MPPercent_A() => pcsc.GetMPPercent_A() * 100f;
        public float MPPercent_B() => pcsc.GetMPPercent_B() * 100f;
    }
}
