using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "WarehouseSO", menuName = "WarehouseSO")]
public class WarehouseSO : ScriptableObject
{
    public int property = 0;
    public List<PropSO> warehouse = new List<PropSO>();
}
