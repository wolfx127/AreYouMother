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
        

        public GameObject GetBullet(Transform ts, float dirX, float dirZ, float length = 120f, float speed = 50f, string target = "Enemy")
        {
            if (pool.Count == 0)
            {
                AddCapacity(20);
            }
            GameObject temp = pool.Dequeue();
            temp.transform.position = ts.position;
            BulletData bullet = temp.GetComponent<BulletData>();
            Vector2 v = new Vector2(dirX, dirZ).normalized;
            dirX = v.x;
            dirZ = v.y;
            bullet.direction = (dirX, dirZ);
            bullet.maxLength = length;
            bullet.Speed = speed;
            bullet.tag = target;
            temp.SetActive(true);
            return temp;
        }

        public void RecycleBullet(GameObject bullet)
        {
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
