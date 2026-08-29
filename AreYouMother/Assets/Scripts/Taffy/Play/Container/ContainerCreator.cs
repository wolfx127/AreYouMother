
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
        containerData.container.Clear();
        for (int i = 0; i < containerData.length; i++)
        {
            containerData.container.Add
            (
                ContainerCreatorTool.GetUnionList
                    (
                        containerData.type,PropRarity.GetRandomRarity()
                    )
                .GetRandomProp()
            );
        }
    }

    private void RandomLength()
    {
        int probability = Random.Range(0,101);
        if (probability < 10)
        {
            containerData.length = 1;
        }
        else if (probability < 25)
        {
            containerData.length = 2;
        }
        else if (probability < 50)
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
