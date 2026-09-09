using System.Collections.Generic;
using TaffyFrame.Trigger;
using UnityEngine;

namespace Taffy.Play.Trigger
{
    public class AttackTrigger : BaseTrigger
    {
        public override List<GameObject> GetVictims()
        {
            return GetVictimList();
        }
    }
}
