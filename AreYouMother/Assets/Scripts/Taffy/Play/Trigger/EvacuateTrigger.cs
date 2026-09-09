using System.Collections.Generic;
using TaffyFrame.Trigger;
using UnityEngine;

namespace Taffy.Play.Trigger
{
    public class EvacuateTrigger : BaseTrigger
    {
        public override List<GameObject> GetVictims()
        {
            return GetVictimList();
        }
    }
}
