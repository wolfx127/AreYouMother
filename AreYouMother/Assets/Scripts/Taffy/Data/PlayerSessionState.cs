using System;
using System.Collections.Generic;
using UnityEngine;

namespace Taffy.Data
{
    public class PlayerSessionState:MonoBehaviour
    {
        public int curHP = 0;
        public int curMP = 0;
        public Prop[] bag;

        private void OnEnable()
        {
            
        }

        private void InitialBaseState(int maxHP,int maxMP,int bagSize)
        {
            curHP = maxHP;
            curMP = maxMP;
            bag = new Prop[bagSize];
        }
    }
}
