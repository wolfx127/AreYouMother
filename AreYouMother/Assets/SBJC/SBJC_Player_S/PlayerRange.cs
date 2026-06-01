using SBJC.SBJC_Player_S;
using UnityEngine;

public class PlayerRange : PlayerController
{
    public Transform firePoint;
    [SerializeField] private float fireHeightOffset = 1.3f;  // 发射起点抬高，避免地面斜坡挡子弹
    protected override void Attack()
    {
        Vector3 spawnPos = firePoint != null ? firePoint.position : transform.position;
        spawnPos.y += fireHeightOffset;

        Quaternion spawnRot = firePoint != null ? firePoint.rotation : transform.rotation;

        GameObject bullet = BulletPool.Instance.Get();
        bullet.transform.position = spawnPos;
        bullet.transform.rotation = spawnRot;
        bullet.GetComponent<Bullet>()
              .Launch(firePoint.forward, false, 0, transform);
    }
}