
using System.Collections.Generic;
using Taffy.Data.PropData;
using Taffy.Play.Container;
using UnityEngine;

public class ContainerCreator : MonoBehaviour
{
    private ContainerData containerData;

    private int size = 5;
    

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
        for (int i = 0; i < size; i++)
        {
            List<Prop> pool =  new List<Prop>();
            while(pool == null || pool.Count == 0)
            {
                pool = ContainerCreatorTool.GetUnionList(containerData.type, PropRarity.GetRandomRarity());
                Debug.Log("没抽中，再抽一次");
            }
            Prop temp = null;
            while (temp == null)
            {
                temp = pool.GetRandomProp();
                Debug.Log("没抽中，再抽一次");
            }
            containerData.AddProp(temp);
        }
    }

    private void RandomLength()
    {
        int probability = Random.Range(0,101);
        if (probability < 5)
        {
            size = 1;
        }
        else if (probability < 15)
        {
            size = 2;
        }
        else if (probability < 40)
        {
            size = 3;
        }
        else if (probability < 80)
        {
            size = 4;
        }
        else size = 5;
    }
}
