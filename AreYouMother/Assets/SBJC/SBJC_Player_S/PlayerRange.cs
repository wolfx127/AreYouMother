using UnityEngine;

public class PlayerRange : PlayerController
{
    public Transform firePoint;
    protected override void Attack()
    {
        GameObject bullet = BulletPool.Instance.Get();
        bullet.transform.position = firePoint.position;
        bullet.transform.rotation = firePoint.rotation;
        bullet.GetComponent<Bullet>()
              .Launch(firePoint.forward);
    }
}