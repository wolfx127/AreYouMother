using System.Collections.Generic;
using UnityEngine;

namespace SBJC.SBJC_Player_S
{
    /// <summary>
    /// 子弹对象池（玩家A 与 敌人共用）。
    /// Get() 取出一颗已激活的子弹；用完由 Bullet 自己调用 Back() 归还。
    /// </summary>
    public class BulletPool : MonoBehaviour
    {
        public static BulletPool Instance;
        [SerializeField] GameObject bulletPrefab;
        [SerializeField] int poolSize = 40;
        [SerializeField] int maxPoolSize = 80;

        private readonly Queue<GameObject> pool = new();
        private int _totalCreated;
        private bool _ready;

        void Awake()
        {
            Instance = this;

            if (bulletPrefab == null)
            {
                Debug.LogError("[BulletPool] 未指定 bulletPrefab");
                return;
            }
            if (bulletPrefab.GetComponent<Bullet>() == null || !bulletPrefab.CompareTag("Bullet"))
            {
                Debug.LogError("[BulletPool] bulletPrefab 必须挂 Bullet 组件且 Tag 为 Bullet");
                return;
            }

            for (int i = 0; i < poolSize; i++)
                pool.Enqueue(CreateOne());

            _totalCreated = poolSize;
            _ready = true;
        }

        private GameObject CreateOne()
        {
            GameObject b = Instantiate(bulletPrefab);
            b.SetActive(false);
            return b;
        }

        /// <summary>
        /// 取一颗未激活的子弹。池空则扩容（不会把还在飞的子弹再发出去）。
        /// </summary>
        public GameObject Get()
        {
            if (!_ready) return null;

            GameObject b;
            if (pool.Count > 0)
            {
                b = pool.Dequeue();
            }
            else if (_totalCreated < maxPoolSize)
            {
                b = CreateOne();
                _totalCreated++;
            }
            else
            {
                Debug.LogWarning("[BulletPool] 池已满（maxPoolSize），无法分配新子弹，请检查是否子弹未被正常回收");
                return null;
            }

            b.SetActive(true);
            return b;
        }

        /// <summary>
        /// 归还子弹：重置并放回队列。由 Bullet.Recycle() 调用。
        /// </summary>
        public void Back(GameObject b)
        {
            if (b == null) return;

            var bullet = b.GetComponent<Bullet>();
            if (bullet == null) return;   // 不是本池的子弹，忽略

            bullet.ResetBullet();
            pool.Enqueue(b);
        }
    }
}
