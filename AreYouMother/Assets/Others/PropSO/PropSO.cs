using Taffy.Data;
using UnityEngine;

[CreateAssetMenu(fileName = "PropSO", menuName = "Prop/PropSO")]
public class PropSO : ScriptableObject
{
    public string name;
    public PropOwner owner;
    public PropBehavior_Value[] behavior_value;
    public string description;
    public int price = 0;
    public Rarity rarity = Rarity.Common;
    public Texture2D image;
}
