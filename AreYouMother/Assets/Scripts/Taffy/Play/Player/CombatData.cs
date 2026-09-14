using System;
using UnityEngine;

namespace Taffy.Play.Player
{
    public class CombatData
    {
        public int maxHP;
        public int maxMP;
        private int _HP;
        private int _MP;
        public int ATK;
        public int DEF;
        public static readonly float HateRadius = 30f;

        public event Action<int> UpdateHPEvent;
        public event Action<int> UpdateMPEvent;

        public int HP
        {
            get => _HP;
            set
            {
                _HP = Mathf.Min(value, maxHP);
                Debug.Log("血量变化");
                UpdateHPEvent?.Invoke(_HP);
            }
        }

        public int MP
        {
            get => _MP;
            set
            {
                _MP = Mathf.Min(value, maxMP);
                Debug.Log("蓝量变化");
                UpdateMPEvent?.Invoke(_MP);
            }
        }

        public CombatData(int HP, int MP, int ATK, int DEF)
        {
            maxHP = HP;
            maxMP = MP;
            this._HP = HP;
            this._MP = MP;
            this.ATK = ATK;
            this.DEF = DEF;
        }

        public void WriteInfo(int HP, int MP, int ATK, int DEF)
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
