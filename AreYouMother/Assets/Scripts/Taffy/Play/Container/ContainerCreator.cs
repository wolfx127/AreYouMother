
using System.Collections.Generic;
using Taffy.Data.PropData;
using Taffy.Play.Container;
using UnityEngine;

public class ContainerCreator : MonoBehaviour
{
    private ContainerData containerData;
    

    private void Awake()
    {
        containerData = gameObject.GetComponent<ContainerData>();
    }

    private void Start()
    {
        Build();
    }
    
    

    private void Build()
    {
        RandomLength();
        containerData.Clear();
        for (int i = 0; i < containerData.length; i++)
        {
            List<Prop> pool = ContainerCreatorTool.GetUnionList(containerData.type, PropRarity.GetRandomRarity());
            if (pool == null || pool.Count == 0) continue;
            containerData.AddProp(pool.GetRandomProp());
        }
    }

    private void RandomLength()
    {
        int probability = Random.Range(0,101);
        if (probability < 5)
        {
            containerData.length = 1;
        }
        else if (probability < 15)
        {
            containerData.length = 2;
        }
        else if (probability < 40)
        {
            containerData.length = 3;
        }
        else if (probability < 80)
        {
            containerData.length = 4;
        }
        else containerData.length = 5;
    }
}
