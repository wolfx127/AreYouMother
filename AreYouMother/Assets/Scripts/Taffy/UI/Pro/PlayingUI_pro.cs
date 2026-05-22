using System.Collections.Generic;
using Taffy.Data;
using Taffy.OverAllManager;
using Taffy.Play.Player;
using UnityEngine;

namespace Taffy.UI.Pro
{
    public class PlayingUI_pro
    {
        /// <summary>
        /// 对外接口应仅用作事件注册
        /// </summary>
        public PlayerCurrentStateController pcsc => PlayerCurrentStateController.Instance;
        /// <summary>
        /// 对外接口应仅用作事件注册
        /// </summary>
        public PlayingHandler_A handlerA => PlayingHandler_A.Instance;
        /// <summary>
        /// 对外接口应仅用作事件注册
        /// </summary>
        public PlayingHandler_B handlerB => PlayingHandler_B.Instance;

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

        public int GetBagCount_A() => pcsc.GetBag_A().Count;
        public int GetBagCount_B() => pcsc.GetBag_B().Count;
        
        public List<Prop> GetBag_A() => pcsc.GetBag_A();
        public List<Prop> GetBag_B() => pcsc.GetBag_B();

        public void RemoveBagAt_A(int i)
        {
            pcsc.RemovePropFromBagByIndex_A(i);
        }

        public void RemoveBagAt_B(int i)
        {
            pcsc.RemovePropFromBagByIndex_B(i);
        }
    }
}
