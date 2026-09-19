using System.Collections.Generic;
using TaffyFrame.Trigger;
using UnityEngine;

namespace Taffy.Play.Trigger
{
    
    
    public class AutoJumpTrigger : BaseTrigger
    {
        public override List<GameObject> GetVictims()
        {
            return GetVictimList();
        }
    }
}
