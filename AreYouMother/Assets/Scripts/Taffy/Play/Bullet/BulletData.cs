using System;
using UnityEngine;

namespace Taffy.Play.Bullet
{
    public class BulletData : MonoBehaviour
    {
        public int ATK;
        public float Speed;
        public (float x, float z) direction;
        private float maxLength = 120;
        private float nowLength = 0;
        
        private void Update()
        {
            gameObject.transform.position += new Vector3(direction.x*Speed*Time.deltaTime, 0, direction.z*Speed*Time.deltaTime);
            nowLength += Speed*Time.deltaTime;
            if (nowLength > maxLength) BulletPool.Instance.RecycleBullet(gameObject);
        }
    }
}
