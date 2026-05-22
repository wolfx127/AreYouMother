using UnityEngine;

public class PlayerMelee : PlayerController
{
    public Transform attackPoint;
    public float radius = 1.2f;
    public int damage = 20; 
    protected override void Attack()
    {
        Collider[] hits = Physics.OverlapSphere
        (
            attackPoint.position,
            radius
        );
        foreach (var hit in hits)
        {
            if (hit.CompareTag("Enemy"))
            {
                //hit.GetComponent<Health>()?.TakeDamage(damage);
            }
        }
    }
}