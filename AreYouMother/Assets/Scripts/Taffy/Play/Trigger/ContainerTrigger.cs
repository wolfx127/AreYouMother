using System.Collections.Generic;
using TaffyFrame.Trigger;
using UnityEngine;

namespace Taffy.Play.Trigger
{
    public class ContainerTrigger : BaseTrigger
    {
        public override List<GameObject> GetVictims()
        {
            return GetVictimList();
        }
    }
}
