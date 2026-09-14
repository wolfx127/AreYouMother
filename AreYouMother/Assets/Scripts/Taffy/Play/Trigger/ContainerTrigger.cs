using System;
using System.Collections.Generic;
using TaffyFrame.Trigger;
using UnityEngine;

namespace Taffy.Play.Trigger
{
    public class ContainerTrigger : BaseTrigger
    {
        public override List<GameObject> GetVictims()
        {
            List<GameObject> list = null;
            if(GetVictimList() != null)
            {
                list = new List<GameObject>();
                foreach (GameObject go in GetVictimList())
                {
                    if(go.CompareTag("Container"))
                    {
                        list.Add(go);
                        return list;
                    }
                }
            }
            return list;
        }
    }
}
