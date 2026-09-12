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
            if(Instance == null)Instance = this;
            if (Instance != this)
            {
                Destroy(Instance);
                Instance = this;
            }
            AddCapacity(50);
        }

        private void AddCapacity(int count)
        {
            if (FormBullet == null)
            {
                Debug.Log("子弹池没设置FormBullet，所以不知道要复制谁");
                return;
            }
            FormBullet.SetActive(false);
            for(int i =  0; i < count; i++)
            {
                GameObject temp = GameObject.Instantiate(FormBullet, transform, false);
                temp.SetActive(false);
                pool.Enqueue(temp);
            }
        }

        public GameObject GetBullet()
        {
            if (pool.Count == 0)
            {
                AddCapacity(20);
            }
            GameObject temp = pool.Dequeue();
            temp.SetActive(true);
            return pool.Dequeue();
        }

        public void RecycleBullet(GameObject bullet)
        {
            bullet.SetActive(false);
            pool.Enqueue(bullet);
        }
    }
}
