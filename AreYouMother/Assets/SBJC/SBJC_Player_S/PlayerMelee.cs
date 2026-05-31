using System.Collections.Generic;
using UnityEngine;

public class PlayerMelee : PlayerController
{
    public Transform attackPoint;
    public float radius = 1.2f;
    public int damage = 20;
    protected override void Attack()
    {
        Vector3 center = attackPoint != null ? attackPoint.position : transform.position;
        Collider[] hits = Physics.OverlapSphere(center, radius);

        // 玩家B近战：只打 EnemyA（去重：一个敌人可能有多个 Collider）
        var hitEnemies = new HashSet<EnemyA>();
        foreach (var hit in hits)
        {
            if (!hit.CompareTag("Enemy")) continue;
            var enemy = hit.GetComponentInParent<EnemyA>();
            if (enemy == null || hitEnemies.Contains(enemy)) continue;
            hitEnemies.Add(enemy);
            enemy.TakeDamage(damage);
        }
    }
}