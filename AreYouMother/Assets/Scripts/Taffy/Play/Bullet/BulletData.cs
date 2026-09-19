using System;
using Taffy.Play.Enemy;
using UnityEngine;

namespace Taffy.Play.Bullet
{
    public class BulletData : MonoBehaviour
    {
        public int ATK;
        public float Speed;
        public (float x, float z) direction;
        public float maxLength = 120;
        public float nowLength = 0;

        public string tag = "Enemy";
        
        private void Update()
        {
            gameObject.transform.position += new Vector3(direction.x*Speed*Time.deltaTime, 0, direction.z*Speed*Time.deltaTime);
            nowLength += Speed*Time.deltaTime;
            if (nowLength > maxLength) BulletPool.Instance.RecycleBullet(gameObject);
        }

        private void OnTriggerEnter(Collider other)
        {
            if(tag == "Enemy" && other.CompareTag(tag))
            {
                Debug.Log($"击中{tag}({other.gameObject})");
                other.GetComponent<EnemyData>().Injury(ATK,new Vector2(direction.x, direction.z).normalized);
                BulletPool.Instance.RecycleBullet(gameObject);
            }
        }
    }
}
