using System;
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
        /// 外部类使用时应仅用作事件注册
        /// </summary>
        public PlayerCurrentStateController pcsc => PlayerCurrentStateController.Instance;
        /// <summary>
        /// 外部类使用时应仅用作事件注册
        /// </summary>
        public PlayingHandler_A handlerA => PlayingHandler_A.Instance;
        /// <summary>
        /// 外部类使用时应仅用作事件注册
        /// </summary>
        public PlayingHandler_B handlerB => PlayingHandler_B.Instance;

        public event Action CheckingProp_AEvent;
        public event Action RemoveBagAt_AEvent;
        public event Action CheckingProp_BEvent;
        public event Action RemoveBagAt_BEvent;

        private int propIndex_A = 0;
        private int prevPropIndex_A = 0;
        private int propIndex_B = 0;
        private int prevPropIndex_B = 0;
        public bool isBagClosed_A => handlerA.isBagClosed;
        public bool isBagClosed_B => handlerB.isBagClosed;

        public void Subscribe()
        {
            handlerA.ChoosePropArrowEvent += GetPropIndex_A;
            handlerA.RemovePropAtEvent += RemoveBagAt_A;
            handlerB.ChoosePropArrowEvent += GetPropIndex_B;
            handlerB.RemovePropAtEvent += RemoveBagAt_B;
        }
        public void Unsubscribe()
        {
            handlerA.ChoosePropArrowEvent -= GetPropIndex_A;
            handlerA.RemovePropAtEvent -= RemoveBagAt_A;
            handlerB.ChoosePropArrowEvent -= GetPropIndex_B;
            handlerB.RemovePropAtEvent -= RemoveBagAt_B;
        }

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

        public Prop GetCurrentProp_A()
        {
            return pcsc.GetBag_A()[propIndex_A];
        }
        public Prop GetCurrentProp_B()
        {
            return pcsc.GetBag_B()[propIndex_B];
        }

        public string GetCurrentPropName_A()
        {
            return GetCurrentProp_A().name;
        }
        public string GetCurrentPropName_B()
        {
            return GetCurrentProp_B().name;
        }

        public string GetCurrentPropDescribe_A()
        {
            return $"价值:{GetCurrentProp_A().value} | 数值:{GetCurrentProp_A().playingQuantity} | {GetCurrentProp_A().rarity}" + '\n' +
                   GetCurrentProp_A().description;
        }
        public string GetCurrentPropDescribe_B()
        {
            return $"价值:{GetCurrentProp_B().value} | 数值:{GetCurrentProp_B().playingQuantity}" + '\n' +
                   GetCurrentProp_B().description;
        }

        public string GetBagInfo_A()
        {
            return $"背包上限/现存道具数:{pcsc.GetBagSize_A()}/{pcsc.GetBag_A().Count}";
        }
        public string GetBagInfo_B()
        {
            return $"背包上限/现存道具数:{pcsc.GetBagSize_B()}/{pcsc.GetBag_B().Count}";
        }

        public int GetPropIndex_A() => propIndex_A;
        public int GetPrevPropIndex_A() => prevPropIndex_A;
        public void SetPrevPropIndex_A(int i) => prevPropIndex_A = i;

        public void SetPropIndex_A(int i)
        {
            propIndex_A = i;
            CheckingProp_AEvent?.Invoke();
        }

        public void RemoveBagAt_A()
        {
            pcsc.RemovePropFromBagByIndex_A(propIndex_A);
            int count = pcsc.GetBag_A().Count;
            if (count == 0) propIndex_A = 0;
            else if (propIndex_A >= count) propIndex_A = count - 1;
            prevPropIndex_A = propIndex_A;
            RemoveBagAt_AEvent?.Invoke();
        }

        public void ClampPropIndex_A()
        {
            int count = pcsc.GetBag_A().Count;
            if (count == 0) { propIndex_A = 0; prevPropIndex_A = 0; return; }
            if (propIndex_A >= count) propIndex_A = count - 1;
            else if (propIndex_A < 0) propIndex_A = 0;
            prevPropIndex_A = propIndex_A;
        }

        public int GetPropIndex_B() => propIndex_B;
        public int GetPrevPropIndex_B() => prevPropIndex_B;
        public void SetPrevPropIndex_B(int i) => prevPropIndex_B = i;

        public void SetPropIndex_B(int i)
        {
            propIndex_B = i;
            CheckingProp_BEvent?.Invoke();
        }

        public void RemoveBagAt_B()
        {
            pcsc.RemovePropFromBagByIndex_B(propIndex_B);
            int count = pcsc.GetBag_B().Count;
            if (count == 0) propIndex_B = 0;
            else if (propIndex_B >= count) propIndex_B = count - 1;
            prevPropIndex_B = propIndex_B;
            RemoveBagAt_BEvent?.Invoke();
        }

        public void ClampPropIndex_B()
        {
            int count = pcsc.GetBag_B().Count;
            if (count == 0) { propIndex_B = 0; prevPropIndex_B = 0; return; }
            if (propIndex_B >= count) propIndex_B = count - 1;
            else if (propIndex_B < 0) propIndex_B = 0;
            prevPropIndex_B = propIndex_B;
        }

        private void GetPropIndex_A(Vector2Int dir)
        {
            int count = pcsc.GetBag_A().Count;
            if (count == 0) return;

            const int cols = 5;
            int row = GetPropIndex_A() / cols;
            int col = GetPropIndex_A() % cols;

            if (dir == Vector2Int.left)
            {
                SetPropIndex_A((GetPropIndex_A() - 1 + count) % count);
            }
            else if (dir == Vector2Int.right)
            {
                SetPropIndex_A((GetPropIndex_A() + 1) % count);
            }
            else if (dir == Vector2Int.up || dir == Vector2Int.down)
            {
                int delta = dir == Vector2Int.up ? -1 : 1;
                // 该列实际有多少行
                int colCount = col < count % cols ? count / cols + 1 : count / cols;
                row = (row + delta + colCount) % colCount;
                SetPropIndex_A(row * cols + col);
            }
        }

        private void GetPropIndex_B(Vector2Int dir)
        {
            int count = pcsc.GetBag_B().Count;
            if (count == 0) return;

            const int cols = 5;
            int row = GetPropIndex_B() / cols;
            int col = GetPropIndex_B() % cols;

            if (dir == Vector2Int.left)
            {
                SetPropIndex_B((GetPropIndex_B() - 1 + count) % count);
            }
            else if (dir == Vector2Int.right)
            {
                SetPropIndex_B((GetPropIndex_B() + 1) % count);
            }
            else if (dir == Vector2Int.up || dir == Vector2Int.down)
            {
                int delta = dir == Vector2Int.up ? -1 : 1;
                int colCount = col < count % cols ? count / cols + 1 : count / cols;
                row = (row + delta + colCount) % colCount;
                SetPropIndex_B(row * cols + col);
            }
        }
    }
}
