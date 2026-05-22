using UnityEngine;

public class PlayerAttack : MonoBehaviour
{
    public KeyCode attackKey;
    public System.Action onAttack;
    void Update()
    {
        if (Input.GetKeyDown(attackKey))
        {
            onAttack?.Invoke();
        }
    }
}

