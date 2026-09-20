using System;
using Taffy.Play.Enemy;
using Taffy.Play.Player;
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

        // 形参默认值必须是编译期常量，所以这里用 Tag 常量而不是字面量
        public const string EnemyTag = "Enemy";
        public const string PlayerTag = "Player";

        /// <summary>箭/子弹模型本身的朝向偏移（度）。默认模型沿 +X 画，绕 Y 转 -90° 后对齐飞行方向。
        /// 如果美术资源不是沿 +X 画的，改这个值就能纠正。</summary>
        public float visualYawOffset = -90f;

        // 由 BulletPool 在出池时赋值；空串表示这颗子弹已失效（在池里）
        public string tag = "";

        private void Update()
        {
            gameObject.transform.position += new Vector3(direction.x*Speed*Time.deltaTime, 0, direction.z*Speed*Time.deltaTime);
            nowLength += Speed*Time.deltaTime;
            if (nowLength > maxLength && BulletPool.Instance != null)
                BulletPool.Instance.RecycleBullet(gameObject);
        }

        /// <summary>
        /// 在 XZ 平面上把模型转向飞行方向。Unity 里绕 Y 正方向旋转会让 +X 轴指向 (sin, 0, cos)，
        /// 所以用 Atan2(x, z) 求 yaw。
        /// </summary>
        public void FaceDirection()
        {
            if (Mathf.Abs(direction.x) < 0.0001f && Mathf.Abs(direction.z) < 0.0001f) return;
            float yaw = Mathf.Atan2(direction.x, direction.z) * Mathf.Rad2Deg;
            transform.rotation = Quaternion.Euler(0f, yaw + visualYawOffset, 0f);
        }

        private void OnTriggerEnter(Collider other)
        {
            if (tag == EnemyTag && other.CompareTag(EnemyTag))
            {
                var enemy = other.GetComponent<EnemyData>();
                if (enemy == null) return;
                Debug.Log($"击中{tag}({other.gameObject})");
                enemy.Injury(ATK, new Vector2(direction.x, direction.z).normalized);
                if (BulletPool.Instance != null) BulletPool.Instance.RecycleBullet(gameObject);
                return;
            }

            if (tag == PlayerTag && other.CompareTag(PlayerTag))
            {
                // 不比对对象名字：能拿到 A 就用 A，拿不到再试 B
                if (other.TryGetComponent(out PlayingHandler_A handlerA))
                {
                    Debug.Log($"击中{tag}({other.gameObject})");
                    handlerA.player.Injury(ATK);
                }
                else if (other.TryGetComponent(out PlayingHandler_B handlerB))
                {
                    Debug.Log($"击中{tag}({other.gameObject})");
                    handlerB.player.Injury(ATK);
                }
                else
                {
                    Debug.LogWarning($"[BulletData] 命中 Player 但取不到 PlayingHandler: {other.name}");
                    return;
                }

                if (BulletPool.Instance != null) BulletPool.Instance.RecycleBullet(gameObject);
            }
        }
    }
}
