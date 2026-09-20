using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Taffy.Play.Bullet
{
    public class BulletPool : MonoBehaviour
    {
        public static BulletPool Instance = null;
        
        public Queue<GameObject> pool = new Queue<GameObject>();
        public GameObject FormBullet = null;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            AddCapacity(50);
        }

        private void AddCapacity(int count)
        {
            if (FormBullet == null)
            {
                Debug.Log("子弹池没设置FormBullet，所以不知道要复制谁");
                return;
            }
            for(int i =  0; i < count; i++)
            {
                GameObject temp = Instantiate(FormBullet, transform, false);
                temp.SetActive(false);
                pool.Enqueue(temp);
            }
        }
        

        /// <summary>
        /// 从池里取一颗子弹。target 决定它能命中谁（"Enemy" 或 "Player"）。
        /// </summary>
        public GameObject GetBullet(Transform ts, float dirX, float dirZ, float length = 120f, float speed = 50f, string target = BulletData.EnemyTag, int atk = 0)
        {
            if (pool.Count == 0)
            {
                AddCapacity(20);
            }
            if (pool.Count == 0) return null;   // FormBullet 没设置，取不出子弹

            GameObject temp = pool.Dequeue();
            temp.transform.position = ts.position;
            BulletData bullet = temp.GetComponent<BulletData>();
            Vector2 v = new Vector2(dirX, dirZ).normalized;
            dirX = v.x;
            dirZ = v.y;
            bullet.direction = (dirX, dirZ);
            bullet.maxLength = length;
            bullet.Speed = speed;
            bullet.ATK = atk;
            bullet.tag = target;
            bullet.nowLength = 0;
            bullet.FaceDirection();   // 出池就摆正朝向，否则箭一直保持上一次的姿态
            temp.SetActive(true);
            return temp;
        }

        public void RecycleBullet(GameObject bullet)
        {
            if (bullet == null) return;
            // 同一步内命中两个目标会重复回收，这里挡住二次入池（否则同一个对象会在队列里出现两次）
            if (!bullet.activeSelf) return;
            bullet.SetActive(false);
            bullet.transform.position = new Vector3(0, 0, 0);
            BulletData bulletData = bullet.GetComponent<BulletData>();
            bulletData.direction = (0, 1);
            bulletData.Speed = 0;
            bulletData.ATK = 0;
            bulletData.tag = "";
            bulletData.nowLength = 0;
            pool.Enqueue(bullet);
        }
    }
}
