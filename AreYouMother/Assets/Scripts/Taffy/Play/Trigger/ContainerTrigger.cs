using System.Collections.Generic;
using TaffyFrame.Trigger;
using UnityEngine;

public class ContainerTrigger : BaseTrigger
{
    public override List<GameObject> GetVictims()
    {
        return GetVictimList();
    }
}
