using UnityEngine;
using UnityEngine.InputSystem;

public abstract class PlayerController : MonoBehaviour
{
    protected PlayerInput input;
    
        protected virtual void Awake()
    {
        var attack = GetComponent<PlayerAttack>();
        attack.onAttack += Attack;
    }

    protected abstract void Attack();
}
