using System.Collections.Generic;
using TaffyFrame.Trigger;
using UnityEngine;

namespace Taffy.Play.Trigger
{
    
    
    public class AutoJumpTrigger : BaseTrigger
    {
        public static readonly float AutoJumpHeight =  1.5f;
        public override List<GameObject> GetVictims()
        {
            return GetVictimList();
        }
    }
}
