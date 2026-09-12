using System.Collections.Generic;
using TaffyFrame.Trigger;
using UnityEngine;

namespace Taffy.Play.Trigger
{
    public class AttackTrigger : BaseTrigger
    {
        public override List<GameObject> GetVictims()
        {
            List<GameObject> list = new List<GameObject>();
            foreach (var go in GetVictimList())
            {
                if (go.CompareTag("Enemy"))
                {
                    list.Add(go);
                }
            }
            return list;
        }
    }
}
